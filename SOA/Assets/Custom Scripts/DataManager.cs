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
    public class DataManager : IDataManager
    {
        //public float updateRateMs;
        //List of all actors in this scenario
        public List<SoaActor> actors = new List<SoaActor>();
        public List<Belief> initializationBeliefs = new List<Belief>();
        public SortedDictionary<int, SoaActor> soaActorDictionary = new SortedDictionary<int,SoaActor>();
        public SortedDictionary<int, SortedDictionary<int, bool>> actorDistanceDictionary = new SortedDictionary<int,SortedDictionary<int,bool>>();

        //Dictionary of belief data
        protected SortedDictionary<Belief.Key, SortedDictionary<int, Belief> > beliefDictionary;
        public System.Object dataManagerLock = new System.Object();
        private ICommManager cm;

        private string room;

        // Constructor
        public DataManager(string roomName, int port=5055)
        {
            room = roomName;
            
            //Initialze belief manager with all environment data, the comms manager will broadcast this
            //data on cm.start()
            
            Serializer ps = new ProtobufSerializer();

            cm = new LocalCommManager(this, ps, new UdpNetwork(port));
            //cm = new PhotonCloudCommManager(this, ps, "app-us.exitgamescloud.com:5055", roomName, 0, 0);
            //cm = new PhotonCloudCommManager(dm, ps, "10.101.5.25:5055", "soa");

            beliefDictionary = new SortedDictionary<Belief.Key, SortedDictionary<int, Belief>>();

            // Note: Comms manager must be started manually after all initial belief processing is done
        }

        public void startComms()
        {
            if (cm != null)
            {
                cm.start();
            }
        }

        public void broadcastBelief(Belief b, int sourceId, int[] recipients)
        {
            if (cm != null)
            {
                cm.addOutgoing(b, sourceId, null);
                
            }
        }

        public string getConnectionInfoForAgent(int agentID)
        {
            return cm.getConnectionForAgent(agentID);
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
                
                SortedDictionary<int, Belief> tempTypeDict = getBeliefsFor(b.getTypeKey());
                if (tempTypeDict != null)
                {
                    Belief oldBelief;
                    if (!getBeliefsFor(b.getTypeKey()).TryGetValue(b.getId(), out oldBelief) || oldBelief.getBeliefTime() < b.getBeliefTime())
                    {
                        getBeliefsFor(b.getTypeKey())[b.getId()] = b;
                    }
                }
                else
                {
                    getBeliefsFor(b.getTypeKey())[b.getId()] = b;
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

            // Allow direct connection between two actors if they can both communicate to the same site.  
            // This is done to get around the fact that custom beliefs are not automatically forwarded
            // and so we run into an issue if a UAV sends a custom belief to a base, base doesn't auto
            // forward, and then the balloon who is only connected to the base never gets the message
            // Note: This fix only allows for a single base relay node
            foreach (SoaActor actor1 in actors)
            {
                foreach (SoaActor actor2 in actors)
                {
                    // Go through each site
                    foreach (SoaActor siteActor in actors)
                    {
                        if (siteActor is SoaSite)
                        {
                            // Relay from actor1 -> siteActor -> actor2
                            if (actorDistanceDictionary[actor1.unique_id][siteActor.unique_id] &&
                                actorDistanceDictionary[siteActor.unique_id][actor2.unique_id])
                            {
                                actorDistanceDictionary[actor1.unique_id][actor2.unique_id] = true;
                            }

                            // Relay from actor2 -> siteActor -> actor1
                            if (actorDistanceDictionary[actor2.unique_id][siteActor.unique_id] &&
                                    actorDistanceDictionary[siteActor.unique_id][actor1.unique_id])
                            {
                                actorDistanceDictionary[actor2.unique_id][actor1.unique_id] = true;
                            }
                        }
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

        public SortedDictionary<Belief.Key, SortedDictionary<int, Belief> > getActorWorldView(int actorId)
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

        public SortedDictionary<Belief.Key, SortedDictionary<int, Belief>> getGodsEyeView()
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

        private SortedDictionary<int, Belief> getBeliefsFor(Belief.BeliefType type)
        {
            return getBeliefsFor(Belief.keyOf(type));
        }

        private SortedDictionary<int, Belief> getBeliefsFor(Belief.Key key)
        {
            SortedDictionary<int, Belief> beliefs;
            if (!beliefDictionary.TryGetValue(key, out beliefs))
            {
                beliefs = new SortedDictionary<int, Belief>();
                beliefDictionary[key] = beliefs;
            }
            return beliefs;
        }
    }
}