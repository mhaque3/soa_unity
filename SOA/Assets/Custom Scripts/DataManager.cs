// Additinonal using statements are needed if we are running in Unity
#if(NOT_UNITY)
#else
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
        public List<SoaActor> actors;
        protected SortedDictionary<int, SoaActor> soaActorDictionary;

        protected SortedDictionary<int, SortedDictionary<int, double>> actorDistanceDictionary;

        //Dictionary of belief data
        protected SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief> > beliefDictionary;
        public System.Object dataManagerLock = new System.Object();
        private PhotonCloudCommManager cm;


        // Constructor
        public DataManager()
        {
            Serializer ps = new ProtobufSerializer();
            cm = new PhotonCloudCommManager(this, ps, "app-us.exitgamescloud.com:5055", "soa", 0);
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

            // Start them
            cm.start();
        }

        #if(NOT_UNITY)
        // Dummy functions for filtering
        public bool filterBelief(Belief b)
        {
            // Everything passes for now
            return true;
        }
        #endif

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
                Debug.Log("DataManager: Received belief of type "
                    + (int)b.getBeliefType() + "\n" + b);

                SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
                if (tempTypeDict != null)
                {
                    Belief oldBelief = beliefDictionary[b.getBeliefType()][b.getId()];
                    if (oldBelief == null || oldBelief.getTime() < b.getTime())
                    {
                        beliefDictionary[b.getBeliefType()][b.getId()] = b;
                    }
                }
                else
                {
                    beliefDictionary[b.getBeliefType()][b.getId()] = b;
                }

                SortedDictionary<int, double> sourceDistanceDictionary = actorDistanceDictionary[sourceId];
                if (sourceDistanceDictionary != null)
                {
                    foreach (KeyValuePair<int, double> entry in sourceDistanceDictionary)
                    {
                        //TODO use actual sensor range
                        SoaActor destActor = soaActorDictionary[entry.Key];
                        double commsRange = destActor.commsRange;
                        if (entry.Value <= commsRange)
                        {
                            destActor.addBelief(b);
                        }
                    }
                }
            }
            #endif
        }

        /*
         * Call this function once every update of the simulation time step to refresh the true position data for all the actors
         */ 
        public void calcualteDistances()
        {
            SortedDictionary<int, Belief> actorDictionary = beliefDictionary[Belief.BeliefType.ACTOR];
            foreach (SoaActor soaActor in actors)
            {

                Belief_Actor actor = (Belief_Actor)actorDictionary[soaActor.unique_id];
                Vector3 actorPos = new Vector3((float)actor.getPos_x(), (float)actor.getPos_y(), (float)actor.getPos_z());

                foreach (SoaActor neighborActor in actors)
                {
                    Belief_Actor neighbor = (Belief_Actor)actorDictionary[neighborActor.unique_id];
                    Vector3 neighborPos = new Vector3((float)neighbor.getPos_x(), (float)neighbor.getPos_y(), (float)neighbor.getPos_z());

                    actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = Math.Sqrt(Math.Pow(actorPos.x - neighborPos.x, 2)
                        + Math.Pow(actorPos.y - neighborPos.y, 2)
                        + Math.Pow(actorPos.z - neighborPos.z, 2));
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
                soaActorDictionary.Add(actor.unique_id, actor);
            }
            else
            {
                Debug.LogError("TRIED TO ADD ACTOR TO DATA MANAGER THAT ALREADY EXISTS");
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
    }
}
