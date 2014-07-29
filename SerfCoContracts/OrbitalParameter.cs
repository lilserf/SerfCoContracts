using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using UnityEngine;

namespace SerfCoContracts
{
    class OrbitalParameter : ContractParameter
    {
        float mTargetAlt;
        float mAltWindow;
        float mTargetInc;
        float mIncWindow;

        public OrbitalParameter()
        {
            mTargetAlt = 100000;
            mAltWindow = 10000;
            mTargetInc = 0;
            mIncWindow = 10;
        }

        public OrbitalParameter(float targetAlt, float altWindow, float targetInc = 0, float incWindow = 10)
        {
            mTargetAlt = targetAlt;
            mAltWindow = altWindow;
            mTargetInc = targetInc;
            mIncWindow = incWindow;
        }

        protected override void OnRegister()
        {
            this.DisableOnStateChange = false;
        }

        protected override void OnUnregister()
        {
        }

        protected override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        protected override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
        }

        protected override string GetTitle()
        {
            var minAlt = mTargetAlt - (mAltWindow / 2);
            var maxAlt = mTargetAlt + (mAltWindow / 2);
            return "Orbital altitude " + minAlt + "-" + maxAlt + ", inclination " + mTargetInc + " (+/- " + mIncWindow / 2 + ")";
        }

        protected override string GetHashString()
        {
            return "OrbitalParameter";
        }

        protected override void OnUpdate()
        {
            var v = FlightGlobals.ActiveVessel;

            if(v != null)
            {
                // Require them to stop accelerating at a stable orbit before we check it
                if(v.ctrlState.mainThrottle > 0)
                {
                    SetIncomplete();
                    return;
                }

                var orbit = v.orbit;

                var minAlt = mTargetAlt - (mAltWindow / 2);
                var maxAlt = mTargetAlt + (mAltWindow / 2);

                if(orbit.ApA < minAlt || orbit.ApA > maxAlt || orbit.PeA < minAlt || orbit.PeA > maxAlt)
                {
                    SetIncomplete();
                }
                else
                {
                    var minInc = mTargetInc - (mIncWindow / 2);
                    var maxInc = mTargetInc + (mIncWindow / 2);
                    if(orbit.inclination < minInc || orbit.inclination > maxInc)
                    {
                        SetIncomplete();
                    }
                    else
                    {
                        SetComplete();
                    }
                }
            }
        }
    }
}
