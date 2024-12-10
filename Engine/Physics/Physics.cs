using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GameEngine
{
    internal class Physics
    {
        const float fixedDeltaTime = 0.01f;
        const float sleepThreshold = 0.01f;

        const int bodyOwnersDictionaryInitialSize = 1000;
        const int collidingPairSetInitialSize = bodyOwnersDictionaryInitialSize * bodyOwnersDictionaryInitialSize / 2;
        const int collidingPairListInitialSize = collidingPairSetInitialSize / 2;

        const int contactsListInitialSize = collidingPairSetInitialSize * 10;

        const int debugCollidingOwnersSetInitialSize = 1000;
        const int debugContactsListInitialSize = collidingPairSetInitialSize * 10;
        const float debugCollidersOpacity = 0.3f;

        static Simulation context;
        static BufferPool bufferPool;

        static float accumulatedDeltaTime;

        static Dictionary<BodyHandle, BodyOwner> nonStaticBodyToOwner;
        static Dictionary<StaticHandle, BodyOwner> staticBodyToOwner;
        static Dictionary<BodyOwner, BodyHandle> ownerToNonStaticBody;
        static Dictionary<BodyOwner, StaticHandle> ownerToStaticBody;

        public enum ForceMode
        {
            force,
            impulse,
            acceleration,
            velocityChange
        };

        public enum ColliderType
        {
            box,
            sphere
        };

        internal struct Contact
        {
            public Vector3 position;
            public Vector3 normal;
            public float depth;
        };

        internal struct Collision
        {
            public Transform transform;
            public Rigidbody rigidbody;
            public List<Contact> contactList;
            public int contactsOffset;
            public int contactsCount;
        };

        public struct BodyOwner
        {
            Rigidbody rigid;
            Trigger trigger;

            public BodyOwner()
            {
                rigid = null;
                trigger = null;
            }

            public Component GetComponent()
            {
                if (rigid == null) { return trigger;  }
                else { return rigid;  }
            }

            public void SetTrigger(Trigger t)
            {
                trigger = t;
                rigid = null;
            }

            public void SetRigidbody(Rigidbody r)
            {
                trigger = null;
                rigid = r;
            }

            public bool IsTrigger()
            {
                return trigger != null;
            }

            public Trigger GetTrigger()
            {
                return trigger;
            }

            public Rigidbody GetRigidbody()
            {
                return rigid;
            }

            public bool Equals(BodyOwner other)
            {
                Component c = (trigger != null ? trigger : rigid);

                return (c == other.GetComponent());
            }

            public override bool Equals(object other)
            {

                if (ReferenceEquals(other, null)) { return false; }

                if (ReferenceEquals(other, this)) { return true; }

                if (other.GetType() != GetType()) { return false; }

                return Equals((BodyOwner)other);
            }

            public override int GetHashCode()
            {
                Component c = trigger != null ? trigger : rigid;
                return c.GetHashCode();
            }
        }

        struct CollidingPair
        {
            public BodyOwner A;
            public BodyOwner B;

            public int contactsOffset;
            public int contactsCount;

            public bool Equals(CollidingPair other)
            {
                bool result = (A.GetComponent() == other.A.GetComponent() && B.GetComponent() == other.B.GetComponent() ||
                               A.GetComponent() == other.B.GetComponent() && B.GetComponent() == other.A.GetComponent());

                //Console.WriteLine("Compared " + "(" + A.GetGameObject().name + ", " + B.GetGameObject().name + ")" + " with " +
                //                                "(" + other.A.GetGameObject().name + ", " + other.B.GetGameObject().name + ")" + " result " + result);

                return result;
            }

            public override bool Equals(object other)
            {
                //Console.WriteLine("Comparing " + this + " with " + other);

                if(ReferenceEquals(other, null)) { return false; }
                
                if(ReferenceEquals(other, this)) { return true; }

                if (other.GetType() != GetType()) { return false; }

                return Equals((CollidingPair)other);
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() / 2 + B.GetHashCode() / 2;
            }

        }

        public struct RaycastHit
        {
            public Transform transform;
            public Vector3 normal;
            public float distance;
            public Vector3 point;
        }


        static HashSet<CollidingPair> collidingPairsSet;
        static List<CollidingPair> collidingPairsList;
        static List<CollidingPair> newCollidingPairs;
        static List<CollidingPair> updatedCollidingPairs;
        static List<CollidingPair> sleepingCollidingPairs;

        static List<Contact> contactsList;
        static List<Contact> previousContactsList;

        static bool renderCollidersEnabled;
        static HashSet<BodyOwner> debugCollidingOwnersSet;
        static List<Contact> debugContactsList;

        struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
        {
            public void Initialize(Simulation simulation)
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
            {

                return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic ||
                       a.Mobility == CollidableMobility.Kinematic || b.Mobility == CollidableMobility.Kinematic;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
            {
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
            {
                int i = 0;
                bool collisionDetected = false;
                BodyOwner ownerA = new BodyOwner();
                BodyOwner ownerB = new BodyOwner();

                int contactsOffset = 0;
                int contactsCount = 0;

                while (i < manifold.Count && !collisionDetected)
                {
                    if(manifold.GetDepth(ref manifold, i) >= -0.001f)
                    {
                        if (pair.A.Mobility == CollidableMobility.Static) { ownerA = staticBodyToOwner[pair.A.StaticHandle]; }
                        else { ownerA = nonStaticBodyToOwner[pair.A.BodyHandle]; }

                        if (pair.B.Mobility == CollidableMobility.Static) { ownerB = staticBodyToOwner[pair.B.StaticHandle]; }
                        else { ownerB = nonStaticBodyToOwner[pair.B.BodyHandle]; }

                        Transform transformA = ownerA.GetComponent().GetGameObject().transform;

                        contactsOffset = contactsList.Count;
                        for(int j = 0; j < manifold.Count; j++)
                        {
                            
                            Vector3 offset;
                            Vector3 normal;
                            float depth;
                            int feature;
                            manifold.GetContact(j, out offset, out normal, out depth, out feature);

                            Contact c = new Contact();
                            c.normal = normal;
                            c.position = transformA.position + offset;
                            c.depth = depth;

                            contactsList.Add(c);
                        }

                        contactsCount = manifold.Count;

                        collisionDetected = true;
                    }
                    else { i++; }
                }

                if(collisionDetected)
                {
                    CollidingPair collidingPair = new CollidingPair();
                    collidingPair.A = ownerA;
                    collidingPair.B = ownerB;
                    collidingPair.contactsOffset = contactsOffset;
                    collidingPair.contactsCount = contactsCount;

                    if(collidingPairsSet.Contains(collidingPair))
                    {
                        updatedCollidingPairs.Add(collidingPair);
                    }
                    else
                    {
                        collidingPairsSet.Add(collidingPair);
                        collidingPairsList.Add(collidingPair);
                        newCollidingPairs.Add(collidingPair);
                    }

                }

                pairMaterial.FrictionCoefficient = 1f;
                pairMaterial.MaximumRecoveryVelocity = 10f;
                pairMaterial.SpringSettings = new SpringSettings(30, 1);


                if(!collisionDetected)
                {
                    return false;
                }
                else if (!ownerA.IsTrigger() && !ownerB.IsTrigger())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
            {
                return true;
            }

            public void Dispose()
            {
            }
        }

        public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
        {
            public void Initialize(Simulation simulation)
            {
            }

            public readonly AngularIntegrationMode AngularIntegrationMode { get { return AngularIntegrationMode.Nonconserving; } }

            public readonly bool AllowSubstepsForUnconstrainedBodies  { get { return false; } }

            public readonly bool IntegrateVelocityForKinematics { get { return false; } }

            public Vector3 Gravity;

            public PoseIntegratorCallbacks(Vector3 gravity) : this()
            {
                Gravity = gravity;
            }

            Vector3Wide gravityWideDt;

            public void PrepareForIntegration(float dt)
            {
                //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
                gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
            }

            public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
            {
                velocity.Linear += gravityWideDt;
            }

        }

        public struct RayHitHandler : IRayHitHandler
        {
            bool hitted;
            float distance;
            Vector3 normal;
            CollidableMobility mobility;
            StaticHandle staticHandle;
            BodyHandle bodyHandle;

            public bool AllowTest(CollidableReference collidable)
            {
                return true;
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return true;
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 _normal, CollidableReference collidable, int childIndex)
            {
                hitted = true;
                distance = t;
                normal = _normal;
                mobility = collidable.Mobility;
                if(mobility == CollidableMobility.Static)
                {
                    staticHandle = collidable.StaticHandle;
                }
                else
                {
                    bodyHandle = collidable.BodyHandle;
                }

            }

            public bool HasHitted()
            {
                return hitted;
            }

            public float GetHitDistance()
            {
                return distance;
            }
            public Vector3 GetHitNormal()
            {
                return normal;
            }

            public bool HittedIsStatic()
            {
                return mobility == CollidableMobility.Static;
            }

            public StaticHandle GetHittedStaticHandle()
            {
                return staticHandle;
            }

            public BodyHandle GetHittedBodyHandle()
            {
                return bodyHandle;
            }
        }

        public static void Init(IWindow window)
        {
            bufferPool = new BufferPool();
            context = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));
            nonStaticBodyToOwner = new Dictionary<BodyHandle, BodyOwner>(bodyOwnersDictionaryInitialSize);
            staticBodyToOwner = new Dictionary<StaticHandle, BodyOwner>(bodyOwnersDictionaryInitialSize);
            ownerToNonStaticBody= new Dictionary<BodyOwner, BodyHandle>(bodyOwnersDictionaryInitialSize);
            ownerToStaticBody = new Dictionary<BodyOwner, StaticHandle>(bodyOwnersDictionaryInitialSize);
            collidingPairsSet = new HashSet<CollidingPair>(collidingPairSetInitialSize);
            collidingPairsList = new List<CollidingPair>(collidingPairListInitialSize);
            sleepingCollidingPairs = new List<CollidingPair>(collidingPairListInitialSize);
            newCollidingPairs = new List<CollidingPair>(collidingPairListInitialSize);
            updatedCollidingPairs = new List<CollidingPair>(collidingPairListInitialSize);
            contactsList = new List<Contact>(contactsListInitialSize);
            previousContactsList = new List<Contact>(contactsListInitialSize);
            accumulatedDeltaTime = 0;

            renderCollidersEnabled = false;
            Render.onRenderOverlay += OnRender;
            debugCollidingOwnersSet = new HashSet<BodyOwner>(debugCollidingOwnersSetInitialSize);
            debugContactsList = new List<Contact>(debugCollidingOwnersSetInitialSize);

        }


        public static void Update(float deltaTime, bool forceSceneFixedUpdate = false)
        {
            bool sceneUpdated = false;

            accumulatedDeltaTime += deltaTime;

            while(accumulatedDeltaTime > fixedDeltaTime)
            {
                contactsList.Clear();

                ///////////////////////////////////////////////////////////////////////
                // The colliding pairs set contains all pairs that have formed       //
                // a manifold at some point, including those pairs that have formed  //
                // a manifold and then gone to sleep.                                //
                ///////////////////////////////////////////////////////////////////////


                updatedCollidingPairs.Clear();
                newCollidingPairs.Clear();

                context.Timestep(fixedDeltaTime);

                ////////////////////////////////////////////////////////////////////////
                // After the timestep all pairs that have formed a manifold are in    //
                // the colliding pairs set. The ones that were before in the set      //
                // have been added also to the updated pairs list and the ones that   //
                // weren't have been added to the new pairs list.                     //
                ////////////////////////////////////////////////////////////////////////
                
                int totalPairs = collidingPairsList.Count;

                ////////////////////////////////////////////////////////////////////////
                // From now on we will remove elements from the colliding set until   //
                // only the pairs that have separated remain there.                   //
                ////////////////////////////////////////////////////////////////////////

                ////////////////////////////////////////////////////////////////////////
                // First we will check if a pair that we have in the set has gone to //
                // sleep and if so, we will remove them from the set and then add it  //
                // to the sleeping colliding pairs list.                              //
                // this guarantees we won't loose track of them so we will send an    //
                // exit event if they become awake again and separate.                //
                // If only one of the bodies of the pair is sleeping the pair will    //
                // show up again in the next timestep so we only have to do this if   //
                // both are sleeping.                                                 //
                ////////////////////////////////////////////////////////////////////////

                sleepingCollidingPairs.Clear();

                for (int i = 0; i < collidingPairsList.Count; i++)
                {
                    CollidingPair p = collidingPairsList[i];

                    bool awakeA = false;
                    bool awakeB = false;

                    if(ownerToNonStaticBody.ContainsKey(p.A))
                    {
                        BodyHandle handle = ownerToNonStaticBody[p.A];
                        awakeA = context.Bodies.GetBodyReference(handle).Awake;
                    }

                    if (ownerToNonStaticBody.ContainsKey(p.B))
                    {
                        BodyHandle handle = ownerToNonStaticBody[p.B];
                        awakeB = context.Bodies.GetBodyReference(handle).Awake;
                    }

                    if (!awakeA && !awakeB)
                    {
                        // Copy contacts to the current contact list
                        int offset = contactsList.Count;

                        for (int j = 0; j < p.contactsCount; j ++)
                        {
                            contactsList.Add(previousContactsList[p.contactsOffset + j]);
                        }

                        p.contactsOffset = offset;

                        sleepingCollidingPairs.Add(p);
                        collidingPairsSet.Remove(p);
                    }

                }

                previousContactsList.Clear();

                /////////////////////////////////////////////////////////////////////////////////////
                // Now we remove also from the set the pairs that have been updated or are new.    //
                // This leaves only in the set the pairs that have disappeared from the set that   //
                // we had before the timestep for a reason different that both members sleeping.   //
                /////////////////////////////////////////////////////////////////////////////////////

                for (int i = 0; i < updatedCollidingPairs.Count; i++)
                {
                    CollidingPair p = updatedCollidingPairs[i];
                    collidingPairsSet.Remove(p);
                }


                for (int i = 0; i < newCollidingPairs.Count; i ++)
                {
                    CollidingPair p = newCollidingPairs[i];
                    collidingPairsSet.Remove(p);
                }

                ////////////////////////////////////////////////////////////////////////////////
                // As the pairs in the set are the ones that have disappeared from the set we //
                // had before the timestep for a reason different than both sleeping we can   //
                // assume that they have separated, so we send exit events                    //
                ////////////////////////////////////////////////////////////////////////////////

                int exitedPairs = 0;

                for(int i = 0; i < collidingPairsList.Count; i++)
                {
                    CollidingPair p = collidingPairsList[i];
                    if(collidingPairsSet.Contains(p))
                    {
                        if(!p.A.IsTrigger() && !p.B.IsTrigger())
                        {
                            Rigidbody rA = p.A.GetRigidbody();
                            Rigidbody rB = p.B.GetRigidbody();
                            Transform tA = p.A.GetRigidbody().GetGameObject().transform;
                            Transform tB = p.B.GetRigidbody().GetGameObject().transform;

                            Collision c = new Collision();

                            c.contactList = contactsList;
                            c.contactsOffset = p.contactsOffset;
                            c.contactsCount = p.contactsCount;

                            c.rigidbody = rB;
                            c.transform = tB;
                            p.A.GetComponent().GetGameObject().OnCollisionExit(c);

                            c.rigidbody = rA;
                            c.transform = tA;
                            p.B.GetComponent().GetGameObject().OnCollisionExit(c);
                        }
                        else if(p.A.IsTrigger() && !p.B.IsTrigger())
                        {
                            Rigidbody rB = p.B.GetRigidbody();
                            p.A.GetComponent().GetGameObject().OnTriggerExit(rB);
                        }
                        else if (p.B.IsTrigger() && !p.A.IsTrigger())
                        {
                            Rigidbody rA = p.A.GetRigidbody();
                            p.B.GetComponent().GetGameObject().OnTriggerExit(rA);
                        }

                        //Console.WriteLine("OnCollisionExit " + p.A.GetGameObject().name + " - " + p.B.GetGameObject().name);
                        exitedPairs++;
                    }

                }

                /////////////////////////////////////////////////////////////////////
                // Now we reset the colliding pairs set and we will add the pairs  //
                // we want to be part of it at the beginning of the next timestep. //
                // As we do so we will send events to them.                        //
                /////////////////////////////////////////////////////////////////////

                collidingPairsSet.Clear();
                collidingPairsList.Clear();

                /////////////////////////////////////////////////////////////////////
                // We add to the set all the updated pairs and we send stay        //
                // events to them                                                  //
                /////////////////////////////////////////////////////////////////////

                for(int i = 0; i < updatedCollidingPairs.Count; i++)
                {
                    CollidingPair p = updatedCollidingPairs[i];

                    collidingPairsSet.Add(p);
                    collidingPairsList.Add(p);

                    if (!p.A.IsTrigger() && !p.B.IsTrigger())
                    {
                        Rigidbody rA = p.A.GetRigidbody();
                        Rigidbody rB = p.B.GetRigidbody();
                        Transform tA = p.A.GetRigidbody().GetGameObject().transform;
                        Transform tB = p.B.GetRigidbody().GetGameObject().transform;

                        Collision c = new Collision();
                        c.contactList = contactsList;
                        c.contactsOffset = p.contactsOffset;
                        c.contactsCount = p.contactsCount;
                        c.rigidbody = rB;
                        c.transform = tB;
                        p.A.GetComponent().GetGameObject().OnCollisionStay(c);
                        c.rigidbody = rA;
                        c.transform = tA;
                        p.B.GetComponent().GetGameObject().OnCollisionStay(c);
                    }
                    else if (p.A.IsTrigger() && !p.B.IsTrigger())
                    {
                        Rigidbody rB = p.B.GetRigidbody();
                        p.A.GetComponent().GetGameObject().OnTriggerStay(rB);
                    }
                    else if (p.B.IsTrigger() && !p.A.IsTrigger())
                    {
                        Rigidbody rA = p.A.GetRigidbody();
                        p.B.GetComponent().GetGameObject().OnTriggerStay(rA);
                    }
                }

                /////////////////////////////////////////////////////////////////////
                // We add to the set all the sleeping pairs and we send stay       //
                // events to them                                                  //
                /////////////////////////////////////////////////////////////////////

                for (int i = 0; i < sleepingCollidingPairs.Count; i++)
                {
                    CollidingPair p = sleepingCollidingPairs[i];

                    collidingPairsSet.Add(p);
                    collidingPairsList.Add(p);

                    if (!p.A.IsTrigger() && !p.B.IsTrigger())
                    {
                        Rigidbody rA = p.A.GetRigidbody();
                        Rigidbody rB = p.B.GetRigidbody();
                        Transform tA = p.A.GetRigidbody().GetGameObject().transform;
                        Transform tB = p.B.GetRigidbody().GetGameObject().transform;

                        Collision c = new Collision();
                        c.contactList = contactsList;
                        c.contactsOffset = p.contactsOffset;
                        c.contactsCount = p.contactsCount;
                        c.rigidbody = rB;
                        c.transform = tB;
                        p.A.GetComponent().GetGameObject().OnCollisionStay(c);
                        c.rigidbody = rA;
                        c.transform = tA;
                        p.B.GetComponent().GetGameObject().OnCollisionStay(c);
                    }
                    else if (p.A.IsTrigger() && !p.B.IsTrigger())
                    {
                        Rigidbody rB = p.B.GetRigidbody();
                        p.A.GetComponent().GetGameObject().OnTriggerStay(rB);
                    }
                    else if (p.B.IsTrigger() && !p.A.IsTrigger())
                    {
                        Rigidbody rA = p.A.GetRigidbody();
                        p.B.GetComponent().GetGameObject().OnTriggerStay(rA);
                    }
                }

                /////////////////////////////////////////////////////////////////////
                // We add to the set all the new pairs and we send enter           //
                // events to them                                                  //
                /////////////////////////////////////////////////////////////////////

                for (int i = 0; i < newCollidingPairs.Count; i++)
                {
                    CollidingPair p = newCollidingPairs[i];

                    collidingPairsSet.Add(p);
                    collidingPairsList.Add(p);

                    if (!p.A.IsTrigger() && !p.B.IsTrigger())
                    {

                        Rigidbody rA = p.A.GetRigidbody();
                        Rigidbody rB = p.B.GetRigidbody();
                        Transform tA = p.A.GetRigidbody().GetGameObject().transform;
                        Transform tB = p.B.GetRigidbody().GetGameObject().transform;

                        Collision c = new Collision();
                        c.contactList = contactsList;
                        c.contactsOffset = p.contactsOffset;
                        c.contactsCount = p.contactsCount;
                        c.rigidbody = rB;
                        c.transform = tB;
                        p.A.GetComponent().GetGameObject().OnCollisionEnter(c);
                        c.rigidbody = rA;
                        c.transform = tA;
                        p.B.GetComponent().GetGameObject().OnCollisionEnter(c);
                    }
                    else if (p.A.IsTrigger() && !p.B.IsTrigger())
                    {
                        Rigidbody rB = p.B.GetRigidbody();
                        p.A.GetComponent().GetGameObject().OnTriggerEnter(rB);
                    }
                    else if (p.B.IsTrigger() && !p.A.IsTrigger())
                    {
                        Rigidbody rA = p.A.GetRigidbody();
                        p.B.GetComponent().GetGameObject().OnTriggerEnter(rA);
                    }
                }

                ////////////////////////////////////////////////////////////
                // There shouldn't be pairs that belong to two of these   //
                // categories (Updated, New, Sleeping and Exited), so     //
                // the amount of pairs that we have in the set after the  //
                // timestep should be equal to the sum of members of them //
                ////////////////////////////////////////////////////////////


                int e = exitedPairs;
                int u = updatedCollidingPairs.Count;
                int n = newCollidingPairs.Count;
                int s = sleepingCollidingPairs.Count;
                int t = totalPairs;
                int unse = u + n + s + e;

                Debug.Assert(t == unse, "Some colliding pairs have been classified in two or more categories");

                //Console.WriteLine(String.Format("U {0:000} N {1:000} S {2:000} E {3:000} UNSE {4:000} T {5:000}", u, n, s, e, unse, t));

                //Console.WriteLine("Contacts " + contactsList.Count);
                //

                previousContactsList.AddRange(contactsList);

                if(renderCollidersEnabled)
                {
                    debugContactsList.Clear();
                    debugContactsList.AddRange(contactsList);
                }
                
                SceneManager.FixedUpdate(fixedDeltaTime);
                sceneUpdated = true;

                accumulatedDeltaTime -= fixedDeltaTime;
            }

            if (forceSceneFixedUpdate && !sceneUpdated) { SceneManager.FixedUpdate(0); }
        }

        public static void SetRenderCollidersEnabled(bool value)
        {
            renderCollidersEnabled = value;
        }

        public static bool Raycast(Vector3 position, Vector3 direction, float maxDistance, out RaycastHit hit)
        {
            RayHitHandler handler = new RayHitHandler();
            context.RayCast<RayHitHandler>(position, direction, maxDistance, ref handler);

            hit = new RaycastHit();

            if (handler.HasHitted())
            {
                hit.normal = handler.GetHitNormal();
                hit.distance = handler.GetHitDistance();
                hit.point = position + direction * handler.GetHitDistance();

                if(handler.HittedIsStatic())
                {
                    StaticHandle handle = handler.GetHittedStaticHandle();
                    hit.transform = staticBodyToOwner[handle].GetComponent().GetGameObject().transform;
                }
                else
                {
                    BodyHandle handle = handler.GetHittedBodyHandle();
                    hit.transform = nonStaticBodyToOwner[handle].GetComponent().GetGameObject().transform;

                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void AddLinearImpulse(Vector3 impulse, BodyHandle handle)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
            bodyRef.ApplyLinearImpulse(impulse);
            bodyRef.Awake = true;

        }

        public static void AddLinearImpulseAt(Vector3 impulse, Vector3 point, BodyHandle handle)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
            bodyRef.ApplyImpulse(impulse, point);
            bodyRef.Awake = true;

        }

        public static void AddVelocity(Vector3 velocity, BodyHandle handle)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
            bodyRef.Velocity.Linear += velocity;
            bodyRef.Awake = true;
        }

        public static void AddAngularImpulse(Vector3 impulse, BodyHandle handle)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
            bodyRef.ApplyAngularImpulse(impulse);
            bodyRef.Awake = true;
        }

        public static void AddAngularVelocity(Vector3 velocity, BodyHandle handle)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
            bodyRef.Velocity.Angular += velocity;
            bodyRef.Awake = true;
        }


        private static void OnRender(float deltaTime)
        {
            if (!renderCollidersEnabled) { return; }

            Render.ClearDepth();
            Render.SetOpacity(debugCollidersOpacity);

            debugCollidingOwnersSet.Clear();
            for (int i = 0; i < collidingPairsList.Count; i++)
            {
                CollidingPair p = collidingPairsList[i];
                debugCollidingOwnersSet.Add(p.A);
                debugCollidingOwnersSet.Add(p.B);
            }

            Texture texture;

            for (int i = 0; i < staticBodyToOwner.Keys.Count; i++)
            {
                StaticHandle staticHandle = staticBodyToOwner.Keys.ElementAt(i);
                BodyOwner owner = staticBodyToOwner[staticHandle];

                StaticReference staticRef = context.Statics.GetStaticReference(staticHandle);
                StaticDescription staticDesc;
                staticRef.GetDescription(out staticDesc);

                if (debugCollidingOwnersSet.Contains(owner))
                {
                    texture = Assets.GetLoadedAsset<Texture>("Yellow.png");
                }
                else if(owner.IsTrigger())
                {
                    texture = Assets.GetLoadedAsset<Texture>("Purple.png");
                }
                else 
                {
                    texture = Assets.GetLoadedAsset<Texture>("Gray.png");
                }

                RenderShape(staticDesc.Pose, staticDesc.Shape, texture);
            }

            for (int i = 0; i < nonStaticBodyToOwner.Keys.Count; i++)
            {
                BodyHandle handle = nonStaticBodyToOwner.Keys.ElementAt(i);
                BodyOwner owner = nonStaticBodyToOwner[handle];

                BodyReference bodyRef = context.Bodies.GetBodyReference(handle);
                BodyDescription bodyDesc;
                bodyRef.GetDescription(out bodyDesc);
                Collidable collidable = bodyRef.Collidable;

                if (bodyRef.Awake)
                {
                    if (debugCollidingOwnersSet.Contains(owner))
                    {
                        texture = Assets.GetLoadedAsset<Texture>("Yellow.png");
                    }
                    else if (owner.IsTrigger())
                    {
                        texture = Assets.GetLoadedAsset<Texture>("Purple.png");
                    }
                    else if (bodyRef.Kinematic)
                    {
                        texture = Assets.GetLoadedAsset<Texture>("Green.png");
                    }
                    else { texture = Assets.GetLoadedAsset<Texture>("Blue.png"); }

                }
                else { texture = Assets.GetLoadedAsset<Texture>("Gray.png"); }

                RenderShape(bodyDesc.Pose, collidable.Shape, texture);


            }

            Model model = Assets.GetLoadedAsset<Model>("UnitBox.obj");
            Shader shader = Assets.GetLoadedAsset<Shader>("DefaultTransparent.shader");
            texture = Assets.GetLoadedAsset<Texture>("Red.png");

            for (int i = 0; i < debugContactsList.Count; i++)
            {
                Contact c = debugContactsList[i];

                Vector3 position = c.position;
                Vector3 rotation = Vector3.Zero;
                Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);

                GameEngine.Render.DrawModel(position, rotation, scale, model, shader, texture);
            }

        }

        static void RenderShape(RigidPose pose, TypedIndex typedIndex, Texture texture)
        {
            Vector3 scale;
            Model model;
            Shader shader = Assets.GetLoadedAsset<Shader>("DefaultTransparent.shader");

            if (typedIndex.Type == Box.Id)
            {
                Box box = context.Shapes.GetShape<Box>(typedIndex.Index);
                model = Assets.GetLoadedAsset<Model>("UnitBox.obj");
                scale = new Vector3(box.HalfWidth, box.HalfHeight, box.HalfLength) * 2;
            }
            else // collidable.Shape.Type == Sphere.Id
            {
                Sphere sphere = context.Shapes.GetShape<Sphere>(typedIndex.Index);
                model = Assets.GetLoadedAsset<Model>("UnitSphere.obj");
                scale = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius) * 2;
            }


            Vector3 position = pose.Position;
            Vector3 rotationRads = MathUtils.QuaternionToEuler(pose.Orientation);
            Vector3 rotation = MathUtils.RadiansToDegrees(rotationRads);

            GameEngine.Render.DrawModel(position, rotation, scale, model, shader, texture);

        }


        public static void Start()
        {
            // Nothing
        }

        public static void Stop()
        {
            Debug.Assert(collidingPairsList.Count == 0, "Colliding pairs list is no empty on stop");
            Debug.Assert(collidingPairsSet.Count == 0, "Colliding pairs set is not empty on stop");

            debugContactsList.Clear();
        }

        public static void Finish()
        {
            Debug.Assert(collidingPairsList.Count == 0, "Colliding pairs list is no empty on finish");
            Debug.Assert(collidingPairsSet.Count == 0, "Colliding pairs set is not empty on finish");
            Debug.Assert(staticBodyToOwner.Count == 0, "Static body to rigidbody dictionary is not empty on finish");
            Debug.Assert(nonStaticBodyToOwner.Count == 0, "Non static body to rigidbody dictionary is not empty on finish");


            Render.onRenderOverlay -= OnRender;

            debugCollidingOwnersSet = null;
            debugContactsList = null;
            previousContactsList = null;
            contactsList = null;
            updatedCollidingPairs = null;
            newCollidingPairs = null;
            sleepingCollidingPairs = null;
            collidingPairsList = null;
            collidingPairsSet = null;
            nonStaticBodyToOwner = null;
            staticBodyToOwner = null;
            ownerToNonStaticBody= null;
            ownerToStaticBody = null;
            context.Dispose();
            bufferPool.Clear();

        }

        public static TypedIndex RegisterSphereCollider(float radius)
        {
            Sphere sphere = new Sphere(radius);
            return context.Shapes.Add(sphere);
        }

        public static TypedIndex RegisterBoxCollider(Vector3 size)
        {
            Box box = new Box(size.X, size.Y, size.Z);

            return context.Shapes.Add(box);
        }

        public static void UnregisterCollider(TypedIndex index)
        {
            context.Shapes.Remove(index);
        }

        public static StaticHandle RegisterStaticBody(Vector3 position, Vector3 rotation, TypedIndex colliderIndex, BodyOwner owner)
        {
            StaticHandle handle;

            Vector3 rotationRads = MathUtils.DegreesToRadians(rotation);
            Quaternion rotationQ = MathUtils.EulerToQuaternion(rotationRads);
            RigidPose pose = new RigidPose(position, rotationQ);

            StaticDescription staticDesc = new StaticDescription(pose, colliderIndex);
            handle = context.Statics.Add(staticDesc);

            staticBodyToOwner[handle] = owner;
            ownerToStaticBody[owner] = handle;

            Console.WriteLine("Registered body " + owner.GetComponent().GetGameObject().name);

            return handle;
        }

        public static BodyHandle RegisterNonStaticBody(Vector3 position, Vector3 rotation, float mass, Vector3 speed, Vector3 angularSpeed, bool isKinematic, TypedIndex colliderIndex, ColliderType colliderType, BodyOwner owner)
        {
            BodyHandle handle;

            Vector3 rotationRads = MathUtils.DegreesToRadians(rotation);
            Quaternion rotationQ = MathUtils.EulerToQuaternion(rotationRads);
            RigidPose pose = new RigidPose(position, rotationQ);

            BodyDescription bodyDesc;
            Vector3 angularSpeedRads = MathUtils.DegreesToRadians(angularSpeed);
            BodyVelocity bodyVelocity = new BodyVelocity(speed, angularSpeedRads);
            BodyActivityDescription bodyActivityDesc = new BodyActivityDescription(sleepThreshold);

            if (isKinematic)
            {
                bodyDesc = BodyDescription.CreateKinematic(pose, bodyVelocity, colliderIndex, bodyActivityDesc);
            }
            else
            {
                BodyInertia inertia;
                if (colliderType == ColliderType.box)
                {
                    Box b = (Box)context.Shapes.GetShape<Box>(colliderIndex.Index);
                    inertia = b.ComputeInertia(mass);
                }
                else
                {
                    Sphere s = (Sphere)context.Shapes.GetShape<Sphere>(colliderIndex.Index);
                    inertia = s.ComputeInertia(mass);
                }

                bodyDesc = BodyDescription.CreateDynamic(pose, bodyVelocity, inertia, colliderIndex, bodyActivityDesc);

            }

            handle = context.Bodies.Add(bodyDesc);

            nonStaticBodyToOwner[handle] = owner;
            ownerToNonStaticBody[owner] = handle;

            Console.WriteLine("Registered body " + owner.GetComponent().GetGameObject().name);

            return handle;
        }

        public static void UnregisterNonStaticBody(BodyHandle handle)
        {
            BodyOwner owner = nonStaticBodyToOwner[handle];
            Predicate<CollidingPair> predicate = (CollidingPair p) => { return p.A.GetComponent() == owner.GetComponent() || p.B.GetComponent() == owner.GetComponent(); };
            collidingPairsList.RemoveAll(predicate);
            collidingPairsSet.RemoveWhere(predicate);

            nonStaticBodyToOwner.Remove(handle);
            ownerToNonStaticBody.Remove(owner);
            context.Bodies.Remove(handle);

            Console.WriteLine("Unregistered body " + owner.GetComponent().GetGameObject().name);
        }

        public static void UnregisterStaticBody(StaticHandle handle)
        {
            BodyOwner owner = staticBodyToOwner[handle];
            Predicate<CollidingPair> predicate = (CollidingPair p) => { return p.A.GetComponent() == owner.GetComponent() || p.B.GetComponent() == owner.GetComponent(); };
            collidingPairsList.RemoveAll(predicate);
            collidingPairsSet.RemoveWhere(predicate);

            ownerToStaticBody.Remove(owner);
            staticBodyToOwner.Remove(handle);
            context.Statics.Remove(handle);

            Console.WriteLine("Unregistered body " + owner.GetComponent().GetGameObject().name);
        }

        public static void GetNonStaticBodyState(BodyHandle handle, out Vector3 position, out Vector3 rotation, out Vector3 speed, out Vector3 angularSpeed)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);

            position = bodyRef.Pose.Position;

            Quaternion rQ = bodyRef.Pose.Orientation;
            Vector3 rRads = MathUtils.QuaternionToEuler(rQ);
            rotation = MathUtils.RadiansToDegrees(rRads);

            speed = bodyRef.Velocity.Linear;
            angularSpeed = MathUtils.RadiansToDegrees(bodyRef.Velocity.Angular);
        }

        public static void SetKinematicBodyState(BodyHandle handle, Vector3 position, Vector3 rotation, Vector3 speed, Vector3 angularSpeed)
        {
            BodyReference bodyRef = context.Bodies.GetBodyReference(handle);

            BodyDescription bodyDesc;
            
            bodyRef.GetDescription(out bodyDesc);

            Vector3 rRads = MathUtils.DegreesToRadians(rotation);
            Quaternion rQ = MathUtils.EulerToQuaternion(rRads);
            RigidPose pose = new RigidPose(position, rQ);


            Vector3 angularSpeedRads = MathUtils.DegreesToRadians(angularSpeed);
            BodyVelocity bodyVelocity = new BodyVelocity(speed, angularSpeedRads);

            bodyDesc.Pose = pose;
            bodyDesc.Velocity = bodyVelocity;

            bodyRef.ApplyDescription(bodyDesc);

        }
    }
}
