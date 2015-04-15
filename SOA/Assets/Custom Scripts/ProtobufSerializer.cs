using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using autonomy_msg;

namespace soa
{
    class ProtobufSerializer : Serializer
    {
        // Last message type must be NUM_MESSAGETYPE
        private enum MessageType
        {
            UNDEFINED = 0,
            ACTOR, 
            BASECELL, 
            MODE_COMMAND,
            NGOSITECELL, 
            NOGO, 
            ROAD, 
            SPOI, 
            TIME,
            VILLAGECELL, 
            WAYPOINT, 
            WAYPOINT_OVERRIDE
        };

        // Constructor
        public ProtobufSerializer()
        {
            // Do nothing
        }

        // Serialize a Belief object into a byte array using Google Protobuf + 1 header byte
        public override byte[] serializeBelief(Belief belief)
        {
            // Variables to hold serialized message
            byte header;
            byte[] body, message;

            // Initialize based on type of belief
            switch (belief.getBeliefType())
            {
                case Belief.BeliefType.ACTOR:
                    { // Actor
                        Gpb_Actor.Builder proto = Gpb_Actor.CreateBuilder();
                        Belief_Actor b = (Belief_Actor)belief;
                        proto.SetUniqueId(b.getUnique_id());
                        proto.SetAffiliation(b.getAffiliation());
                        proto.SetType(b.getType());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        proto.SetPosZ(b.getPos_z());
                        // Optional fields
                        if (b.getVelocity_x_valid())
                        {
                            proto.SetVelocityX(b.getVelocity_x());
                        }
                        if (b.getVelocity_y_valid())
                        {
                            proto.SetVelocityY(b.getVelocity_y());
                        }
                        if (b.getVelocity_z_valid())
                        {
                            proto.SetVelocityZ(b.getVelocity_z());
                        }
                        // Form header + serialized message
                        header = (byte)MessageType.ACTOR;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.BASECELL:
                    { // Base Cell
                        Gpb_BaseCell.Builder proto = Gpb_BaseCell.CreateBuilder();
                        Belief_BaseCell b = (Belief_BaseCell)belief;
                        proto.SetId(b.getId());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.BASECELL;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.MODE_COMMAND:
                    { // Mode Command
                        Gpb_Mode_Command.Builder proto = Gpb_Mode_Command.CreateBuilder();
                        Belief_Mode_Command b = (Belief_Mode_Command)belief;
                        proto.SetRequestTime(b.getRequest_time());
                        proto.SetActorId(b.getActor_id());
                        proto.SetModeId(b.getMode_id());
                        // Form header + serialized message
                        header = (byte)MessageType.MODE_COMMAND;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.NGOSITECELL:
                    { // NGO Site Cell
                        Gpb_NGOSiteCell.Builder proto = Gpb_NGOSiteCell.CreateBuilder();
                        Belief_NGOSiteCell b = (Belief_NGOSiteCell)belief;
                        proto.SetId(b.getId());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.NGOSITECELL;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.NOGO:
                    { // No Go
                        Gpb_Nogo.Builder proto = Gpb_Nogo.CreateBuilder();
                        Belief_Nogo b = (Belief_Nogo)belief;
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.NOGO;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.ROAD:
                    { // Road
                        Gpb_Road.Builder proto = Gpb_Road.CreateBuilder();
                        Belief_Road b = (Belief_Road)belief;
                        proto.SetIsRoadEnd(b.getIsRoadEnd());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.ROAD;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.SPOI:
                    { // SPOI
                        Gpb_SPOI.Builder proto = Gpb_SPOI.CreateBuilder();
                        Belief_SPOI b = (Belief_SPOI)belief;
                        proto.SetRequestTime(b.getRequest_time());
                        proto.SetActorId(b.getActor_id());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.SPOI;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.TIME:
                    { // Time
                        Gpb_Time.Builder proto = Gpb_Time.CreateBuilder();
                        Belief_Time b = (Belief_Time)belief;
                        proto.SetTime(b.getTime());
                        // Form header + serialized message
                        header = (byte)MessageType.TIME;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.VILLAGECELL:
                    { // Village Cell
                        Gpb_VillageCell.Builder proto = Gpb_VillageCell.CreateBuilder();
                        Belief_VillageCell b = (Belief_VillageCell)belief;
                        proto.SetId(b.getId());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        // Form header + serialized message
                        header = (byte)MessageType.VILLAGECELL;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.WAYPOINT:
                    { // Waypoint
                        Gpb_Waypoint.Builder proto = Gpb_Waypoint.CreateBuilder();
                        Belief_Waypoint b = (Belief_Waypoint)belief;
                        proto.SetRequestTime(b.getRequest_time());
                        proto.SetActorId(b.getActor_id());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        proto.SetPosZ(b.getPos_z());
                        // Form header + serialized message
                        header = (byte)MessageType.WAYPOINT;
                        body = proto.Build().ToByteArray();
                        break;
                    }
                case Belief.BeliefType.WAYPOINT_OVERRIDE:
                    { // Waypoint Override
                        Gpb_Waypoint_Override.Builder proto = Gpb_Waypoint_Override.CreateBuilder();
                        Belief_Waypoint_Override b = (Belief_Waypoint_Override)belief;
                        proto.SetRequestTime(b.getRequest_time());
                        proto.SetActorId(b.getActor_id());
                        proto.SetPosX(b.getPos_x());
                        proto.SetPosY(b.getPos_y());
                        proto.SetPosZ(b.getPos_z());
                        // Form header + serialized message
                        header = (byte)MessageType.WAYPOINT_OVERRIDE;
                        body = proto.Build().ToByteArray();
                        break;
                    }

                default:
                    // Unrecognized type, return empty array
                    Console.Error.WriteLine("ProtobufSerializer.serializeBelief(): Unrecognized Belief type ");
                    return new byte[0];
            }

            // Return serialized message (header + body)
            message = new Byte[body.Length + 1];
            message[0] = header;
            System.Buffer.BlockCopy(body, 0, message, 1, body.Length);
            return message;
        }

        // Deserialize
        public override Belief generateBelief(byte[] serial)
        {
            // New belief that we will return
            Belief b;

            // Break string into header and body
            MessageType headerType = (MessageType) serial[0];
            Byte[] body = new Byte[serial.Length-1];
            System.Buffer.BlockCopy(serial, 1, body, 0, serial.Length - 1);

            // Initialize based on type of belief
            switch (headerType)
            {
                case MessageType.ACTOR:
                    { // Actor
                        Gpb_Actor proto = Gpb_Actor.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Actor(
                            proto.UniqueId,
                            proto.Affiliation,
                            proto.Type,
                            proto.PosX,
                            proto.PosY,
                            proto.PosZ,
                            proto.HasVelocityX,
                            proto.VelocityX,
                            proto.HasVelocityY,
                            proto.VelocityY,
                            proto.HasVelocityZ,
                            proto.VelocityZ);
                        break;
                    }
                case MessageType.BASECELL:
                    { // Base Cell
                        Gpb_BaseCell proto = Gpb_BaseCell.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_BaseCell(
                            proto.Id,
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.MODE_COMMAND:
                    { // Mode Command
                        Gpb_Mode_Command proto = Gpb_Mode_Command.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Mode_Command(
                            proto.RequestTime,
                            proto.ActorId,
                            proto.ModeId);
                        break;
                    }
                case MessageType.NGOSITECELL:
                    { // NGO Site Cell
                        Gpb_NGOSiteCell proto = Gpb_NGOSiteCell.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_NGOSiteCell(
                            proto.Id,
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.NOGO:
                    { // No Go
                        Gpb_Nogo proto = Gpb_Nogo.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Nogo(
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.ROAD:
                    { // Road
                        Gpb_Road proto = Gpb_Road.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Road(
                            proto.IsRoadEnd,
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.SPOI:
                    { // SPOI
                        Gpb_SPOI proto = Gpb_SPOI.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_SPOI(
                            proto.RequestTime,
                            proto.ActorId,
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.TIME:
                    { // Time
                        Gpb_Time proto = Gpb_Time.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Time(
                            proto.Time);
                        break;
                    }
                case MessageType.VILLAGECELL:
                    { // Village Cell
                        Gpb_VillageCell proto = Gpb_VillageCell.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_VillageCell(
                            proto.Id,
                            proto.PosX,
                            proto.PosY);
                        break;
                    }
                case MessageType.WAYPOINT:
                    { // Waypoint
                        Gpb_Waypoint proto = Gpb_Waypoint.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Waypoint(
                            proto.RequestTime,
                            proto.ActorId,
                            proto.PosX,
                            proto.PosY,
                            proto.PosZ);
                        break;
                    }
                case MessageType.WAYPOINT_OVERRIDE:
                    { // Waypoint Override
                        Gpb_Waypoint_Override proto = Gpb_Waypoint_Override.CreateBuilder().MergeFrom(body).Build();
                        b = new Belief_Waypoint_Override(
                            proto.RequestTime,
                            proto.ActorId,
                            proto.PosX,
                            proto.PosY,
                            proto.PosZ);
                        break;
                    }
                default:
                    // Unrecognized type
                    Console.Error.WriteLine("ProtobufSerializer.generateBelief(): Unrecognized header type " + headerType );
                    // Don't create a new belief object and return to caller
                    return null;
            }

            // Return the created belief object
            return b;
        }
    }
}
