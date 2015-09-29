using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace soa
{
    public class Belief_Actor : Belief
    {
        // Members
        private int unique_id;
        private int affiliation;
        private int type;
        private bool isAlive;
        private int isCarrying;
        private bool isWeaponized;
        private float fuelRemaining;
        private float pos_x;
        private float pos_y;
        private float pos_z;
        private bool velocity_x_valid;
        private float velocity_x;
        private bool velocity_y_valid;
        private float velocity_y;
        private bool velocity_z_valid;
        private float velocity_z;

        // Constructor
        public Belief_Actor(int unique_id, int affiliation, int type,
            bool isAlive, int isCarrying, bool isWeaponized, float fuelRemaining,
            float pos_x, float pos_y, float pos_z,
            bool velocity_x_valid = false, float velocity_x = 0.0f,
            bool velocity_y_valid = false, float velocity_y = 0.0f,
            bool velocity_z_valid = false, float velocity_z = 0.0f): base(unique_id)
        {
            this.unique_id = unique_id;
            this.affiliation = affiliation;
            this.type = type;
            this.isAlive = isAlive;
            this.isCarrying = isCarrying;
            this.isWeaponized = isWeaponized;
            this.fuelRemaining = fuelRemaining;
            this.pos_x = pos_x;
            this.pos_y = pos_y;
            this.pos_z = pos_z;
            this.velocity_x_valid = velocity_x_valid;
            this.velocity_x = velocity_x;
            this.velocity_y_valid = velocity_y_valid;
            this.velocity_y = velocity_y;
            this.velocity_z_valid = velocity_z_valid;
            this.velocity_z = velocity_z;
        }

        // Type information
        public override BeliefType getBeliefType()
        {
            return BeliefType.ACTOR;
        }

        // String representation
        public override string ToString()
        {
            string s = "Belief_Actor {"
                + "\n" + "  unique_id: " + unique_id
                + "\n" + "  affiliation: " + affiliation
                + "\n" + "  type: " + type
                + "\n" + "  isAlive: " + isAlive
                + "\n" + "  isCarrying: " + isCarrying
                + "\n" + "  isWeaponized: " + isWeaponized
                + "\n" + "  fuelRemaining: " + fuelRemaining
                + "\n" + "  pos_x: " + pos_x
                + "\n" + "  pos_y: " + pos_y
                + "\n" + "  pos_z: " + pos_z
                + "\n" + "  velocity_x_valid: " + velocity_x_valid
                + "\n" + "  velocity_x: " + velocity_x
                + "\n" + "  velocity_y_valid: " + velocity_y_valid
                + "\n" + "  velocity_y: " + velocity_y
                + "\n" + "  velocity_z_valid: " + velocity_z_valid
                + "\n" + "  velocity_z: " + velocity_z
                + "\n" + "}";
            return s;
        }

        // Get methods
        public int getUnique_id() { return unique_id; }
        public int getAffiliation() { return affiliation; }
        public int getType() { return type; }
        public bool getIsAlive() { return isAlive; }
        public int getIsCarrying() { return isCarrying; }
        public bool getIsWeaponized() { return isWeaponized; }
        public float getFuelRemaining() { return fuelRemaining; }
        public float getPos_x() { return pos_x; }
        public float getPos_y() { return pos_y; }
        public float getPos_z() { return pos_z; }
        public bool getVelocity_x_valid() { return velocity_x_valid; }
        public float getVelocity_x() { return velocity_x; }
        public bool getVelocity_y_valid() { return velocity_y_valid; }
        public float getVelocity_y() { return velocity_y; }
        public bool getVelocity_z_valid() { return velocity_z_valid; }
        public float getVelocity_z() { return velocity_z; }
    }
}
