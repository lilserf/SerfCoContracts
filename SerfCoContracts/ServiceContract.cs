using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Contracts;
using UnityEngine;

namespace SerfCoContracts
{
    class ServiceContract : Contract
    {
        Vessel mTargetSat;
        String mTargetName;

        protected override bool Generate()
        {
            var vessels = GetValidVessels();
            var count = vessels.Count();
            int index = UnityEngine.Random.Range(0,count);
            
            mTargetSat = vessels[index];
            Debug.Log("[ServiceContract] Generate: chose satellite " + mTargetSat.vesselName + " [" + mTargetSat.id + "]");
            if(mTargetSat == null)
            {
                return false;
            }

            mTargetName = mTargetSat.vesselName;

            base.SetExpiry();
            base.SetDeadlineYears(2);
            base.SetFunds(1000, 1000);
            base.SetReputation(10);
            base.SetScience(100);

            AddParameter(new Contracts.Parameters.KerbalDeaths());

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }

        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return "SatelliteService";
        }


        protected override string GetTitle()
        {
            return "Service satellite "+mTargetName;
        }

        protected override string GetSynopsys()
        {
            return "Dock with existing satellite " + mTargetName;
        }

        protected override string GetDescription()
        {
            return "We need you to dock with existing satellite " + mTargetName;
        }

        protected override string MessageCompleted()
        {
            return "You docked with existing satellite " + mTargetName + "!";
        }

        protected override void OnLoad(ConfigNode node)
        {
            string guid = node.GetValue("targetSat");
            Guid id = new Guid(guid);
            mTargetSat = FlightGlobals.Vessels.FirstOrDefault(v => v.id.Equals(id));

            mTargetName = node.GetValue("targetName");

            if(mTargetSat != null)
            {
                mTargetName = mTargetSat.vesselName;
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("targetSat", mTargetSat.id.ToString());
            node.AddValue("targetName", mTargetName);
        }

        private IList<Vessel> GetValidVessels()
        {
            // Get unmanned orbiting vessels that aren't debris or asteroids
            var vessels = FlightGlobals.Vessels
                .Where(v => v.vesselType != VesselType.Debris && v.vesselType != VesselType.SpaceObject)
                .Where(v => v.situation == Vessel.Situations.ORBITING)
                .Where(v => v.GetCrewCapacity() == 0);

            List<Vessel> validVessels = new List<Vessel>();

            // Now search through the packed part list (since these vessels are not actually simulated right now)
            // and find any parts that implement the ModuleDockingNode module
            foreach(var v in vessels)
            {
                foreach(var pps in v.protoVessel.protoPartSnapshots)
                {
                    if(pps.modules.Where(p => p.moduleName == "ModuleDockingNode").Count() > 0)
                    {
                        validVessels.Add(v);
                        break;
                    }
                }
            }
            
            return validVessels;
        }

        public override bool MeetRequirements()
        {
            var vessels = GetValidVessels();
            return vessels.Count() > 0;
        }
    }
}
