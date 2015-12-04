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
        private UInt32 numStorageSlots;
        private UInt32 numCasualtiesStored;
        private UInt32 numSuppliesStored;
        private UInt32 numCiviliansStored;
        private bool isWeaponized;
        private bool hasJammer;
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
        public Belief_Actor(int unique_id, int affiliation, int type, bool isAlive, 
            UInt32 numStorageSlots, UInt32 numCasualtiesStored,
            UInt32 numSuppliesStored, UInt32 numCiviliansStored, 
            bool isWeaponized, bool hasJammer, float fuelRemaining,
            float pos_x, float pos_y, float pos_z,
            bool velocity_x_valid = false, float velocity_x = 0.0f,
            bool velocity_y_valid = false, float velocity_y = 0.0f,
            bool velocity_z_valid = false, float velocity_z = 0.0f): base(unique_id)
        {
            this.unique_id = unique_id;
            this.affiliation = affiliation;
            this.type = type;
            this.isAlive = isAlive;
            this.numStorageSlots = numStorageSlots;
            this.numCasualtiesStored = numCasualtiesStored;
            this.numSuppliesStored = numSuppliesStored;
            this.numCiviliansStored = numCiviliansStored;
            this.isWeaponized = isWeaponized;
	        this.hasJammer = hasJammer;
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
                + "\n" + "  numStorageSlots: " + numStorageSlots
                + "\n" + "  numCasualtiesStored: " + numCasualtiesStored
                + "\n" + "  numSuppliesStored: " + numSuppliesStored
                + "\n" + "  numCiviliansStored: " + numCiviliansStored
                + "\n" + "  isWeaponized: " + isWeaponized
                + "\n" + "  hasJammer: " + hasJammer
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
        public UInt32 getNumStorageSlots() { return numStorageSlots; }
        public UInt32 getNumCasualtiesStored() { return numCasualtiesStored; }
        public UInt32 getNumSuppliesStored() { return numSuppliesStored; }
        public UInt32 getNumCiviliansStored() { return numCiviliansStored; }
        public bool getIsWeaponized() { return isWeaponized; }
        public bool getHasJammer() { return hasJammer; }
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
