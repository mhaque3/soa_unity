// Additinonal using statements are needed if we are running in Unity
#if(UNITY_STANDALONE)
using UnityEngine;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class DataManager
    {
        //public float updateRateMs;
        //List of all actors in this scenario
        public List<SoaActor> actors = new List<SoaActor>();
        protected SortedDictionary<int, SoaActor> soaActorDictionary = new SortedDictionary<int,SoaActor>();

        protected SortedDictionary<int, SortedDictionary<int, bool>> actorDistanceDictionary = new SortedDictionary<int,SortedDictionary<int,bool>>();

        //Dictionary of belief data
        protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;
        public System.Object dataManagerLock = new System.Object();
        private PhotonCloudCommManager cm;

        private string room;

        // Constructor
        public DataManager(string roomName)
        {
            room = roomName;
            Serializer ps = new ProtobufSerializer();
            cm = new PhotonCloudCommManager(this, ps, "app-us.exitgamescloud.com:5055", roomName, 0);
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

            // Start them
            cm.start();

            beliefDictionary = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
            beliefDictionary[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();
        }

        public void addAndBroadcastBelief(Belief b, int sourceId)
        {
            cm.addOutgoing(b, sourceId);
            addBelief(b, sourceId);
            
        }

        // Check if belief is newer than current belief of matching type and id, if so,
        // replace old belief with b.
        public void addBelief(Belief b, int sourceId)
        {
            lock (dataManagerLock)
            {
#if(NOT_UNITY)
            Console.WriteLine("DataManager: Received belief of type "
                + (int)b.getBeliefType() + "\n" + b);
#else
                //Debug.Log("DataManager: Received belief of type " + (int)b.getBeliefType() + "\n" + b.ToString());
#endif
                SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
                if (tempTypeDict != null)
                {
                    Belief oldBelief;
                    if (!beliefDictionary[b.getBeliefType()].TryGetValue(b.getId(), out oldBelief) || oldBelief.getBeliefTime() < b.getBeliefTime())
                    {
                        beliefDictionary[b.getBeliefType()][b.getId()] = b;
                    }
                }
                else
                {
                    beliefDictionary[b.getBeliefType()][b.getId()] = b;
                }

                SortedDictionary<int, bool> sourceDistanceDictionary;
                
                if (actorDistanceDictionary.TryGetValue(sourceId, out sourceDistanceDictionary))
                {
                    foreach (KeyValuePair<int, bool> entry in sourceDistanceDictionary)
                    {
                        SoaActor destActor = soaActorDictionary[entry.Key];
                        if (entry.Value)
                        {
                            destActor.addBelief(b);
                        }
                    }
                }
            }
        }

        /*
         * Call this function once every update of the simulation time step to refresh the true position data for all the actors
         * A pair of actors actorDistanceDictionary[destId][sourceId] = true if the source comms range is large enough to reach the destination.
         */
        public void calcualteDistances()
        {
            SortedDictionary<int, Belief> actorDictionary = beliefDictionary[Belief.BeliefType.ACTOR];
            foreach (SoaActor soaActor in actors)
            {

                
                //Debug.Log("looking up actor  " + soaActor.unique_id);
                Belief b;
                if (actorDictionary.TryGetValue(soaActor.unique_id, out b))
                {
                    Belief_Actor actor = (Belief_Actor)b;
                    Vector3 actorPos = new Vector3((float)actor.getPos_x(), (float)actor.getPos_y(), (float)actor.getPos_z());

                    foreach (SoaActor neighborActor in actors)
                    {
                        //Debug.Log("looking up actor " + soaActor.unique_id);
                        Belief_Actor neighbor = (Belief_Actor)actorDictionary[neighborActor.unique_id];
                        Vector3 neighborPos = new Vector3((float)neighbor.getPos_x(), (float)neighbor.getPos_y(), (float)neighbor.getPos_z());

                        actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = Math.Sqrt(Math.Pow(actorPos.x - neighborPos.x, 2)
                            + Math.Pow(actorPos.y - neighborPos.y, 2)
                            + Math.Pow(actorPos.z - neighborPos.z, 2)) < neighborActor.commsRange;
                    }
                }
                else
                {
                    Debug.LogError("Could not find actor " + soaActor.unique_id + " in " + room);
                }
            }
        }

        /*
         * Add every new actor to the data manager list of actors
         */
        public void addNewActor(SoaActor actor)
        {
            if (!actors.Contains(actor))
            {
                actors.Add(actor);
                Debug.Log("Adding actor to actor dictionary " + actor.unique_id);
                soaActorDictionary[actor.unique_id] = actor;
                actorDistanceDictionary[actor.unique_id] = new SortedDictionary<int,bool>();
                addBelief(new Belief_Actor(actor.unique_id, actor.affiliation, actor.type,
                    actor.displayPosition.x, actor.displayPosition.y, actor.displayPosition.z), actor.unique_id);
            }
            else
            {
                Debug.LogError("TRIED TO ADD ACTOR TO DATA MANAGER THAT ALREADY EXISTS: " + actor.unique_id);
            }
        }

        public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > getActorWorldView(int actorId)
        {

            SoaActor soaActor = soaActorDictionary[actorId];
            if (soaActor != null)
            {
                return soaActor.getBeliefDictionary();
            }
            else
            {
                Debug.LogError("getActorWorldView actor id " + actorId + " does not exist.");
                return null;
            }
        }

        public SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>> getGodsEyeView()
        {
            return beliefDictionary;
        }

        public void stopPhoton()
        {
            if (cm != null)
                cm.terminate();
        }

        // This method returns all Beliefs that a new player needs to initialize itself
        // such as map/terrain description, etc.  These beliefs do not change over time.
        public List<Belief> getInitializationBeliefs()
        {
            // Beliefs will be stored in a list
            List<Belief> l = new List<Belief>();

            // Add in initialization beliefs
            l.AddRange(beliefDictionary[Belief.BeliefType.BASE].Values);
            l.AddRange(beliefDictionary[Belief.BeliefType.GRIDSPEC].Values);
            l.AddRange(beliefDictionary[Belief.BeliefType.NGOSITE].Values);
            l.AddRange(beliefDictionary[Belief.BeliefType.ROADCELL].Values);
            l.AddRange(beliefDictionary[Belief.BeliefType.TERRAIN].Values);
            l.AddRange(beliefDictionary[Belief.BeliefType.VILLAGE].Values);

            // Return to caller
            return l;
        }
    }
}