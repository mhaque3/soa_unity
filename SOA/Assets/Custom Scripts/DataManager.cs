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
        public List<Belief> initializationBeliefs = new List<Belief>();
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
            
            //Initialze belief manager with all environment data, the comms manager will broadcast this
            //data on cm.start()
            
            Serializer ps = new ProtobufSerializer();
            cm = new PhotonCloudCommManager(this, ps, "app-us.exitgamescloud.com:5055", roomName, 0);
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

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

            // Note: Comms manager must be started manually after all initial belief processing is done
        }

        public void startComms()
        {
            cm.start();
        }

        public void addAndBroadcastBelief(Belief b, int sourceId)
        {
            cm.addOutgoing(b, sourceId, null);
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
                // Get the dictionary for that belief type
                SortedDictionary<int, Belief> tempTypeDict = beliefDictionary[b.getBeliefType()];
                bool updateDictionary;
                Belief oldBelief;
                if (tempTypeDict != null && beliefDictionary[b.getBeliefType()].TryGetValue(b.getId(), out oldBelief))
                {
                    // We are in here if a previous belief already exists and we have to merge
                    if (b.getBeliefType() == Belief.BeliefType.ACTOR)
                    {
                        // Convert to belief actors
                        Belief_Actor oldActorBelief = (Belief_Actor) oldBelief;
                        Belief_Actor incomingActorBelief = (Belief_Actor) b;

                        // To keep track of what to merge
                        bool useIncomingClassification = false;
                        bool useIncomingData = false;

                        // Check which classification to use
                        if(oldActorBelief.getAffiliation() == (int)Affiliation.UNCLASSIFIED && 
                            incomingActorBelief.getAffiliation() != (int)Affiliation.UNCLASSIFIED){
                                // Incoming belief has new classification information
                                useIncomingClassification = true;
                        }

                        // Check which data to use
                        if(incomingActorBelief.getBeliefTime() > oldActorBelief.getBeliefTime()){
                            // Incoming belief has new data information
                            useIncomingData = true;
                        }

                        // Merge based on what was new
                        if (!useIncomingClassification && !useIncomingData)
                        {
                            // No new classification or new data, just ignore the incoming belief
                            updateDictionary = false;
                        }
                        else if (!useIncomingClassification && useIncomingData)
                        {
                            // Keep existing classification and just take incoming data
                            updateDictionary = true;
                            b = new Belief_Actor(
                                incomingActorBelief.getUnique_id(),
                                oldActorBelief.getAffiliation(),
                                incomingActorBelief.getType(),
                                incomingActorBelief.getIsAlive(),
                                incomingActorBelief.getIsCarrying(),
                                oldActorBelief.getIsWeaponized(),
                                incomingActorBelief.getPos_x(),
                                incomingActorBelief.getPos_y(),
                                incomingActorBelief.getPos_z(),
                                incomingActorBelief.getVelocity_x_valid(),
                                incomingActorBelief.getVelocity_x(),
                                incomingActorBelief.getVelocity_y_valid(),
                                incomingActorBelief.getVelocity_y(),
                                incomingActorBelief.getVelocity_z_valid(),
                                incomingActorBelief.getVelocity_z());
                            b.setBeliefTime(incomingActorBelief.getBeliefTime());
                        }
                        else if (useIncomingClassification && !useIncomingData)
                        {
                            // Use incoming classification but keep existing data
                            updateDictionary = true;
                            b = new Belief_Actor(
                                oldActorBelief.getUnique_id(),
                                incomingActorBelief.getAffiliation(),
                                oldActorBelief.getType(),
                                oldActorBelief.getIsAlive(),
                                oldActorBelief.getIsCarrying(),
                                incomingActorBelief.getIsWeaponized(),
                                oldActorBelief.getPos_x(),
                                oldActorBelief.getPos_y(),
                                oldActorBelief.getPos_z(),
                                oldActorBelief.getVelocity_x_valid(),
                                oldActorBelief.getVelocity_x(),
                                oldActorBelief.getVelocity_y_valid(),
                                oldActorBelief.getVelocity_y(),
                                oldActorBelief.getVelocity_z_valid(),
                                oldActorBelief.getVelocity_z());
                            b.setBeliefTime(oldActorBelief.getBeliefTime());
                        }
                        else
                        {
                            // Use all of the incoming belief
                            updateDictionary = true;
                            b = incomingActorBelief;
                        }
                    }
                    else
                    {
                        // General merge policy (take newest belief) for every belief except actor  
                        if (oldBelief.getBeliefTime() < b.getBeliefTime())
                        {
                            updateDictionary = true;
                        }
                        else
                        {
                            updateDictionary = false;
                        }
                    }
                }
                else
                {
                    // Nothing in the dictionary for this belief type, put new entry in
                    updateDictionary = true;
                }

                // Update the dictionary entry if necessary
                if (updateDictionary)
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
                    // Get own position in km
                    Belief_Actor actor = (Belief_Actor)b;
                    Vector3 actorPos = new Vector3(
                        (float)actor.getPos_x(),
                        (float)actor.getPos_y(),
                        (float)actor.getPos_z());

                    foreach (SoaActor neighborActor in actors)
                    {
                        // Get neighbor position in km
                        //Debug.Log("looking up actor " + soaActor.unique_id);
                        Belief_Actor neighbor = (Belief_Actor)actorDictionary[neighborActor.unique_id];
                        Vector3 neighborPos = new Vector3(
                            (float)neighbor.getPos_x(),
                            (float)neighbor.getPos_y(),
                            (float)neighbor.getPos_z());

                        // Compute distance in km and compare against comms range (also specified in km)
                        actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = 
                            Math.Sqrt(Math.Pow(actorPos.x - neighborPos.x, 2)
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
                Debug.Log("Adding actor " + actor.unique_id + " to actor dictionary");
                soaActorDictionary[actor.unique_id] = actor;
                actorDistanceDictionary[actor.unique_id] = new SortedDictionary<int,bool>();
                addBelief(new Belief_Actor(actor.unique_id, (int)actor.affiliation, actor.type, 
                    actor.isAlive, (int)actor.isCarrying, actor.isWeaponized,
                    actor.displayPosition.x / SimControl.KmToUnity,
                    actor.displayPosition.y / SimControl.KmToUnity,
                    actor.displayPosition.z / SimControl.KmToUnity), 
                    actor.unique_id);
            }
            else
            {
                Debug.LogError("TRIED TO ADD ACTOR TO DATA MANAGER THAT ALREADY EXISTS: " + actor.unique_id);
            }
        }

        /*
         * Remove an existing actor from data manager list of actors
         */
        public void removeActor(SoaActor actor)
        {
            if (actors.Contains(actor))
            {
                actors.Remove(actor);
                Debug.Log("Removing actor " + actor.unique_id + " from actor dictionary");
                soaActorDictionary.Remove(actor.unique_id);

                // Remove from both source and destination of distance dictionary
                actorDistanceDictionary.Remove(actor.unique_id);
                foreach (SortedDictionary<int,bool> d in actorDistanceDictionary.Values)
                {
                    d.Remove(actor.unique_id);
                }
            }
            else
            {
                Debug.LogError("TRIED TO REMOVE ACTOR FROM DATA MANAGER THAT DOESN'T EXIST: " + actor.unique_id);
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

        // Initialization beliefs are kept separately from actual belief manager beliefs
        // in case a player joins mid game
        public void addInitializationBelief(Belief b)
        {
            initializationBeliefs.Add(b);
        }

        // This method returns all Beliefs that a new player needs to initialize itself
        // such as map/terrain description, etc.  These beliefs do not change over time.
        public List<Belief> getInitializationBeliefs()
        {
            return initializationBeliefs;
        }
    }
}