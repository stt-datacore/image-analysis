/*
 Copyright (C) 2019 TemporalAgent7 <https://github.com/TemporalAgent7>

 This file is part of the DataCore Bot open source project.

 This program is free software; you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation; either version 3 of the License, or
 (at your option) any later version.

 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Lesser General Public License for more details.

 You should have received a copy of the GNU Lesser General Public License
 along with DataCore Bot; if not, see <http://www.gnu.org/licenses/>.

 The algorithm in this file was originally developed by Chewable C++ and released
 without any license restrictions; see <https://codepen.io/somnivore/pen/Nabyzw>
*/
using System;
using System.Linq;
using System.Collections.Generic;

namespace DataCore.Library
{
    public class ExtendResult
    {
        public double result { get; set; }
        public double safeResult { get; set; }
        public double saferResult { get; set; }
        public double lastDil { get; set; }
        public double dilChance { get; set; }
        public double refillCostResult { get; set; }
    }

    public static class VoyageCalculator
    {
        public static List<ExtendResult> CalculateVoyage(int ps, int ss, int o1, int o2, int o3, int o4, int startAm, double elapsedHours = 0)
        {
            var RND = new Random();

            var numExtends = 2;
            var maxExtends = 100;
            var maxNum20hourSims = 100;
            var ticksPerCycle = 28;
            var secondsPerTick = 20;
            var secondsInMinute = 60;
            var minutesInHour = 60;
            var hazardTick = 4;
            var rewardTick = 7;
            var hazardAsRewardTick = 28;
            var ticksPerMinute = secondsInMinute / secondsPerTick;
            var ticksPerHour = ticksPerMinute * minutesInHour;
            var cycleSeconds = ticksPerCycle * secondsPerTick;
            var cyclesPerHour = minutesInHour * secondsInMinute / cycleSeconds;
            var hazPerCycle = 6;
            var amPerActivity = 1;
            var activityPerCycle = 18;
            var hoursBetweenDilemmas = 2;
            var dilemmasPerHour = 1 / hoursBetweenDilemmas;
            var ticksBetweenDilemmas = hoursBetweenDilemmas * minutesInHour * ticksPerMinute;
            var hazPerHour = hazPerCycle * cyclesPerHour - dilemmasPerHour;
            var hazSkillPerHour = 1260;
            var hazSkillPerTick = hazSkillPerHour / ticksPerHour;
            var hazAmPass = 5;
            var hazAmFail = 30;
            var activityAmPerHour = activityPerCycle * cyclesPerHour * amPerActivity;
            var psChance = 0.35;
            var ssChance = 0.25;
            var osChance = 0.1;
            double[] skillChances = { psChance, ssChance, osChance, osChance, osChance, osChance };
            var dilPerMin = 5;

            var hazSkillVariance = 20 / 100;
            var numSims = 5000;
            var currentAm = 0;
            var ship = startAm;

            var num20hourSims = Math.Min(maxNum20hourSims, numSims);

            var elapsedHazSkill = elapsedHours * hazSkillPerHour;

            int[] skills = { ps, ss, o1, o2, o3, o4 };
            double maxSkill = skills.Max();
            maxSkill = Math.Max(0, maxSkill - elapsedHazSkill);
            var endVoySkill = maxSkill * (1 + hazSkillVariance);


            var results = new List<List<double>>();
            var resultsRefillCostTotal = new List<double>();
            for (int iExtend = 0; iExtend <= numExtends; ++iExtend)
            {
                var resultsEntry = new List<double>(numSims);
                for (int i = 0; i <= numSims; ++i)
                {
                    resultsEntry.Add(0);
                }
                results.Add(resultsEntry);
                resultsRefillCostTotal.Add(0);
            }

            double results20hrCostTotal = 0;
            double results20hrRefillsTotal = 0;

            for (int iSim = 0; iSim < numSims; iSim++)
            {
                var tick = Math.Floor(elapsedHours * ticksPerHour);
                var am = ship;
                double refillCostTotal = 0;
                var extend = 0;

                while (true)
                {
                    ++tick;
                    // sanity escape:
                    if (tick == 10000)
                        break;

                    // hazard && not dilemma
                    if (tick % hazardTick == 0
                        && tick % hazardAsRewardTick != 0
                        && tick % ticksBetweenDilemmas != 0)
                    {
                        var hazDiff = tick * hazSkillPerTick;

                        // pick the skill
                        var skillPickRoll = RND.NextDouble();
                        int skill;
                        if (skillPickRoll < psChance)
                        {
                            skill = ps;
                        }
                        else if (skillPickRoll < psChance + ssChance)
                        {
                            skill = ss;
                        }
                        else
                        {
                            skill = skills[2 + RND.Next(0, 3)];
                        }

                        // check (roll if necessary)
                        var skillVar = hazSkillVariance * skill;
                        var skillMin = skill - skillVar;
                        if (hazDiff < skillMin)
                        { // automatic success
                            am += hazAmPass;
                        }
                        else
                        {
                            var skillMax = skill + skillVar;
                            if (hazDiff >= skillMax)
                            { // automatic fail
                                am -= hazAmFail;
                            }
                            else
                            { // roll for it
                                var skillRoll = RND.Next(skillMin, skillMax);
                                //test.text += minSkill + "-" + maxSkill + "=" + skillRoll + " "
                                if (skillRoll >= hazDiff)
                                {
                                    am += hazAmPass;
                                }
                                else
                                {
                                    am -= hazAmFail;
                                }
                            }
                        }
                    }
                    else if (tick % rewardTick != 0
                             && tick % hazardAsRewardTick != 0
                             && tick % ticksBetweenDilemmas != 0)
                    {
                        am -= amPerActivity;
                    }

                    if (am <= 0)
                    { // system failure
                        if (extend == maxExtends)
                            break;

                        var voyTime = tick / ticksPerHour;
                        var refillCost = Math.Ceiling(voyTime * 60 / dilPerMin);

                        if (extend <= numExtends)
                        {
                            results[extend][iSim] = tick / ticksPerHour;
                            if (extend > 0)
                            {
                                resultsRefillCostTotal[extend] += refillCostTotal;
                            }
                        }

                        am = startAm;
                        refillCostTotal += refillCost;
                        extend++;

                        if (voyTime > 20)
                        {
                            results20hrCostTotal += refillCostTotal;
                            results20hrRefillsTotal += extend;
                            break;
                        }

                        if (extend > numExtends && iSim >= num20hourSims)
                        {
                            break;
                        }
                    } // system failure
                } // foreach tick
            } // foreach sim

            List<ExtendResult> extendResults = new List<ExtendResult>();
            for (int extend = 0; extend <= numExtends; ++extend)
            {
                var exResults = results[extend];

                exResults.Sort();
                var voyTime = exResults[(int)Math.Floor((double)(exResults.Count / 2))];

                // compute other results
                var safeTime = exResults[(int)Math.Floor((double)(exResults.Count / 10))];
                var saferTime = exResults[(int)Math.Floor((double)(exResults.Count / 100))];
                var safestTime = exResults[0];

                // compute last dilemma chance
                double lastDilemma = 0;
                var lastDilemmaFails = 0;
                for (var i = 0; i < exResults.Count; i++)
                {
                    var dilemma = Math.Floor(exResults[i] / hoursBetweenDilemmas);
                    if (dilemma > lastDilemma)
                    {
                        lastDilemma = dilemma;
                        lastDilemmaFails = Math.Max(0, i);
                    }
                }

                var dilChance = Math.Round((double)(100 * (exResults.Count - lastDilemmaFails) / exResults.Count));
                // HACK: if there is a tiny chance of the next dilemma, assume 100% chance of the previous one instead
                if (dilChance == 0)
                {
                    lastDilemma--;
                    dilChance = 100;
                }

                ExtendResult extendResult = new ExtendResult();
                extendResult.result = voyTime;
                extendResult.safeResult = safeTime;
                extendResult.saferResult = saferTime;
                extendResult.lastDil = lastDilemma * hoursBetweenDilemmas;
                extendResult.dilChance = dilChance;
                if (extend > 0)
                {
                    extendResult.refillCostResult = Math.Ceiling(resultsRefillCostTotal[extend] / numSims);
                }

                // the threshold here is just a guess
                if (maxSkill / hazSkillPerHour > voyTime)
                {
                    var tp = Math.Floor(voyTime * hazSkillPerHour);
                    if (currentAm == 0)
                    {
                        //setWarning(extend, "Your highest skill is too high by about " + Math.floor(maxSkill - voyTime*hazSkillPerHour) + ". To maximize voyage time, redistribute more like this: " + tp + "/" + tp + "/" + tp/4 + "/" + tp/4 + "/" + tp/4 + "/" + tp/4 + ".");
                    }
                }

                extendResults.Add(extendResult);
            } // foreach extend

            //20 hour cost (dil) = Math.ceil(results20hrCostTotal/num20hourSims);
            //for this many refills = Math.round(results20hrRefillsTotal/num20hourSims);

            return extendResults;
        }
    }
}