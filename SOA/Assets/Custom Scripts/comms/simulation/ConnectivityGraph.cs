using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace soa
{
    class ConnectivityGraph
    {
        private const int MAX_DEPTH = 10000;
        private readonly List<SoaActor> actors;
        private Dictionary<int, ActorNode> nodes;
        private SortedDictionary<int, SortedDictionary<int, bool>> actorDistanceDictionary = new SortedDictionary<int, SortedDictionary<int, bool>>();

        public ConnectivityGraph()
        {
            this.actors = new List<SoaActor>();
            this.nodes = new Dictionary<int, ActorNode>();
        }

        public void addActor(SoaActor actor)
        {
            if (!actors.Contains(actor))
            {
                actors.Add(actor);
            }
        }

        public void RemoveActor(SoaActor actor)
        {
            actors.Remove(actor);
        }

        public void UpdateConnectivity()
        {
            lock(nodes)
            {
                initializeNodes();
                computeNeighbors();
            }
        }

        public IEnumerable<SoaActor> GetActorsInCliqueOf(int actorID)
        {
            HashSet<SoaActor> clique = new HashSet<SoaActor>();
            ActorNode node = nodes[actorID];
            BuildClique(clique, node, 0);
            return clique;

        }

        private void BuildClique(HashSet<SoaActor> clique, ActorNode node, int depth)
        {
            if (depth > MAX_DEPTH)
            {
                return;
            }

            foreach (ActorNode neighbor in node.neighbors)
            {
                if (clique.Add(neighbor.actor))
                {
                    BuildClique(clique, neighbor, depth + 1);
                }
            }
        }

        private void initializeNodes()
        {
            nodes.Clear();
            foreach (SoaActor actor in actors)
            {
                nodes.Add(actor.unique_id, new ActorNode(actor));
            }
        }

        private void computeNeighbors()
        {
            foreach (SoaActor soaActor in actors)
            {
                ActorNode node = nodes[soaActor.unique_id];

                foreach (SoaActor neighborActor in actors)
                {
                    if (soaActor == neighborActor)
                        continue;

                    ActorNode neighborNode = nodes[neighborActor.unique_id];

                    if (canCommunicate(node, neighborNode))
                    {
                        node.addNeighbor(neighborNode);
                    }
                }
            }
        }

        private bool canCommunicate(ActorNode actor, ActorNode neighbor)
        {
            if (hasWiredConnection(actor, neighbor))
            {
                return true;
            }
            
            float rx_tx_range_km = Vector3.Distance(actor.actorPos_km, neighbor.actorPos_km);
            float rangeSquared_km2 = rx_tx_range_km * rx_tx_range_km;
            float commsRange_km = actor.actor.commsRange_km;
            float snr = (commsRange_km * commsRange_km) / (rangeSquared_km2 * (1 + actor.jammerNoiseSummation));
            return snr >= 1;
        }

        private bool hasWiredConnection(ActorNode actor, ActorNode neighbor)
        {
            return (actor.isBalloon() && neighbor.isBaseStation())
                    || (actor.isBaseStation() && neighbor.isBalloon());
        }

        private class ActorNode
        {
            public readonly SoaActor actor;
            public readonly List<ActorNode> neighbors;//list because iterating is more important than insertion
            public readonly Vector3 actorPos_km;
            public readonly float jammerNoiseSummation;

            public ActorNode(SoaActor actor)
            {
                this.actor = actor;
                this.neighbors = new List<ActorNode>();
                this.actorPos_km = new Vector3(
                    actor.gameObject.transform.position.x / SimControl.KmToUnity,
                    actor.simAltitude_km,
                    actor.gameObject.transform.position.z / SimControl.KmToUnity);
                this.jammerNoiseSummation = computeJammerNoise();
            }

            public void addNeighbor(ActorNode node)
            {
                if (!neighbors.Contains(node))
                {
                    neighbors.Add(node);
                }
            }

            public bool isBalloon()
            {
                return actor.type == (int)SoaActor.ActorType.BALLOON;
            }

            public bool isBaseStation()
            {
                return actor is SoaSite;
            }

            private float computeJammerNoise()
            {
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
                return jammerNoiseSummation;
            }
        }
    }
}
