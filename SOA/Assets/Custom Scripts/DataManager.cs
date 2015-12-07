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
        public SortedDictionary<int, SoaActor> soaActorDictionary = new SortedDictionary<int,SoaActor>();

        public SortedDictionary<int, SortedDictionary<int, bool>> actorDistanceDictionary = new SortedDictionary<int,SortedDictionary<int,bool>>();

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

             cm = new PhotonCloudCommManager(this, ps, "app-us.exitgamescloud.com:5055", roomName, 0, 0);
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

            beliefDictionary = new SortedDictionary<Belief.BeliefType, SortedDictionary<int, Belief>>();
            beliefDictionary[Belief.BeliefType.ACTOR] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.BASE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.CASUALTY_DELIVERY] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.CASUALTY_PICKUP] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.GRIDSPEC] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.INVALID] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.MODE_COMMAND] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.NGOSITE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.ROADCELL] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.SPOI] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.SUPPLY_DELIVERY] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.SUPPLY_PICKUP] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.TERRAIN] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.TIME] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.VILLAGE] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.WAYPOINT] = new SortedDictionary<int, Belief>();
            beliefDictionary[Belief.BeliefType.WAYPOINT_OVERRIDE] = new SortedDictionary<int, Belief>();

            // Note: Comms manager must be started manually after all initial belief processing is done
        }

        public void startComms()
        {
            if (cm != null)
            cm.start();
        }

        public void broadcastBelief(Belief b, int sourceId, int[] recipients)
        {
            if (cm != null)
            {
                cm.addOutgoing(b, sourceId, null);
                
            }
        }

        /*public void addAndBroadcastBelief(Belief b, int sourceId, int[] recipients)
        {
            if (cm != null)
            {
                cm.addOutgoing(b, sourceId, recipients);
                addBelief(b, sourceId);
            }
            
        }*/

        //Add incoming belief to correct agent
        public void addExternalBeliefToActor(Belief b, int sourceId)
        {
            SoaActor a;
            soaActorDictionary.TryGetValue(sourceId, out a);
            if (a != null)
            {
                a.addBeliefToBeliefDictionary(b);
            }
        }

        public void addBeliefToAllActors(Belief b, int sourceId)
        {
            foreach (KeyValuePair<int, SoaActor> entry in soaActorDictionary)
            {
                entry.Value.addBeliefToUnmergedBeliefDictionary(b);
            }
        }


        // Check if belief is newer than current belief of matching type and id, if so,
        // replace old belief with b.
        public void addBeliefToDataManager(Belief b, int sourceId)
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

                /*
                 * Do not update actors in this function
                SortedDictionary<int, bool> sourceDistanceDictionary;
                if (actorDistanceDictionary.TryGetValue(sourceId, out sourceDistanceDictionary))
                {
                    foreach (KeyValuePair<int, bool> entry in sourceDistanceDictionary)
                    {
                        SoaActor destActor = soaActorDictionary[entry.Key];
                        if (entry.Value)
                        {
                            destActor.addBelief(b, sourceId);
                        }
                    }
                }*/
            }
        }

        /*
         * Call this function once every update of the simulation time step to refresh the true position data for all the actors
         * A pair of actors actorDistanceDictionary[destId][sourceId] = true if the source comms range is large enough to reach the destination.
         */
        public void calculateDistances()
        {
            foreach (SoaActor soaActor in actors)
            {                
                // Get own truth coordinates in km
                Vector3 actorPos_km = new Vector3(
                    soaActor.gameObject.transform.position.x / SimControl.KmToUnity,
                    soaActor.simAltitude_km,
                    soaActor.gameObject.transform.position.z / SimControl.KmToUnity);

                float jammerNoiseSummation = 0;
                foreach (SoaJammer jammerActor in SimControl.jammers) 
                {
                    // Get jammer truth coordinates in km
                    Vector3 jammerPos_km = new Vector3(
                        jammerActor.gameObject.transform.position.x / SimControl.KmToUnity,
                        jammerActor.GetComponentInParent<SoaActor>().simAltitude_km,
                        jammerActor.gameObject.transform.position.z / SimControl.KmToUnity);
                    float jammerToActorDist_km = Vector3.Distance(actorPos_km, jammerPos_km);

                    // Sum jammer's noise contribution to actor's SNR
                    jammerNoiseSummation += (jammerActor.effectiveRange_km * jammerActor.effectiveRange_km) / (jammerToActorDist_km * jammerToActorDist_km);
                }

                foreach (SoaActor neighborActor in actors)
                {
                    // Add in exception for balloons
                    if (soaActor.type == (int)SoaActor.ActorType.BALLOON || neighborActor.type == (int)SoaActor.ActorType.BALLOON)
                    {
                        if (soaActor is SoaSite || neighborActor is SoaSite)
                        {
                            // Balloon and blue base comms always established (blue base is the only soasite)
                            actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = true;
                        }
                        else
                        {
                            // Balloons cant talk to anyone else except for blue base
                            actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = false;
                        }
                    }
                    else
                    {
                        // Get neighbor truth coordinates in km
                        Vector3 neighborPos_km = new Vector3(
                            neighborActor.gameObject.transform.position.x / SimControl.KmToUnity,
                            neighborActor.simAltitude_km,
                            neighborActor.gameObject.transform.position.z / SimControl.KmToUnity);

                        float rx_tx_range_km = Vector3.Distance(actorPos_km, neighborPos_km);
                        float rangeSquared_km2 = rx_tx_range_km * rx_tx_range_km;
                        float snr = (soaActor.commsRange_km * soaActor.commsRange_km) / (rangeSquared_km2 * (1 + jammerNoiseSummation));

                        // Compare calculated SNR value to 1.  Comms are 100% reliable at 1
                        actorDistanceDictionary[soaActor.unique_id][neighborActor.unique_id] = (snr >= 1);
                    }
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
                //Debug.Log("Adding actor " + actor.unique_id + " to actor dictionary");
                soaActorDictionary[actor.unique_id] = actor;
                actorDistanceDictionary[actor.unique_id] = new SortedDictionary<int,bool>();

                addBeliefToDataManager(
                    new Belief_Actor(actor.unique_id, (int)actor.affiliation, actor.type, actor.isAlive, 
                        actor.numStorageSlots, actor.numCasualtiesStored,
                        actor.numSuppliesStored, actor.numCiviliansStored,
                        actor.isWeaponized, actor.hasJammer, actor.fuelRemaining_s,
                        actor.transform.position.x / SimControl.KmToUnity,
                        actor.simAltitude_km,
                        actor.transform.position.z / SimControl.KmToUnity), 
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