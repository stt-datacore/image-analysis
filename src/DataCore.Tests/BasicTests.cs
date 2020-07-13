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
*/
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DataCore.Library;

namespace DataCore.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void TestBotHelper()
        {
            var botHelper = new BotHelper("https://datacore.app/", Path.Combine(Directory.GetCurrentDirectory(), "../../../../../data"));
            botHelper.ParseData();

            Assert.AreNotEqual(0, botHelper.TotalCrew());

            var crewResults = botHelper.SearchCrew("data");
            Assert.AreNotEqual(0, crewResults.Count);

            crewResults = botHelper.SearchCrew("skywalker");
            Assert.AreEqual(0, crewResults.Count);

            var itemResults = botHelper.SearchItem("combadge");
            Assert.AreNotEqual(0, itemResults.Count);

            itemResults = botHelper.SearchItem("lightsaber");
            Assert.AreEqual(0, itemResults.Count);

            var dilemmaResults = botHelper.SearchDilemmas("Champion");
            Assert.AreNotEqual(0, dilemmaResults.Count);

            dilemmaResults = botHelper.SearchDilemmas("Jedi");
            Assert.AreEqual(0, dilemmaResults.Count);

            var crew = botHelper.GetCrew("jedi_master");
            Assert.AreEqual(null, crew);

            crew = botHelper.GetCrew("data_detective_crew");
            Assert.AreNotEqual(null, crew);
            Assert.AreEqual("Detective Data", crew.name);
            Assert.AreNotEqual(null, crew.base_skills.engineering_skill);
            Assert.AreEqual(null, crew.base_skills.medicine_skill);
            Assert.IsTrue(crew.skill_data[0].base_skills.engineering_skill.core < crew.skill_data[1].base_skills.engineering_skill.core);
            Assert.AreNotEqual(0, crew.totalChronCost);

            var bestCrewResults = botHelper.BestCrew("base cmd", 0);
            Assert.AreNotEqual(0, bestCrewResults.Count);

            bestCrewResults = botHelper.BestCrew("gauntlet cmd", 0);
            Assert.AreNotEqual(0, bestCrewResults.Count);

            bestCrewResults = botHelper.BestCrew("gauntlet cmd sci", 0);
            Assert.AreNotEqual(0, bestCrewResults.Count);

            bestCrewResults = botHelper.BestCrew("voyage cmd", 0);
            Assert.AreNotEqual(0, bestCrewResults.Count);

            bestCrewResults = botHelper.BestCrew("voyage cmd sci", 0);
            Assert.AreNotEqual(0, bestCrewResults.Count);

            bestCrewResults = botHelper.BestCrew("bases cmd", 0);
            Assert.AreEqual(null, bestCrewResults);

            bestCrewResults = botHelper.BestCrew("voyage cmd sci sec", 0);
            Assert.AreEqual(null, bestCrewResults);

            var gauntletCrew = botHelper.Gauntlet(new string[] {"borg", "interrogator", "resourceful"});
            Assert.AreNotEqual(0, gauntletCrew.Results.Length);

            gauntletCrew = botHelper.Gauntlet(new string[] {"jedi", "invalid", "bogus"});
            Assert.IsFalse(string.IsNullOrEmpty(gauntletCrew.ErrorMessage));
        }

        [TestMethod]
        public void TestVoyageCalculator()
        {
            var results = VoyageCalculator.CalculateVoyage(10000, 10000, 3000, 3000, 3000, 3000, 2500);
            Assert.AreNotEqual(0, results.Count);
            Assert.IsTrue(results[0].result > 8);
        }

        [TestMethod]
        public void TestBeholdImageParser()
        {
            Searcher searcher = new Searcher(Path.Combine(Directory.GetCurrentDirectory(), "../../../../.."));
            Assert.AreNotEqual(null, searcher);

            var result = searcher.Search(Path.Combine(Directory.GetCurrentDirectory(), "../../../fixtures/behold1.png"));
            Assert.IsTrue(result.IsValid());
            Assert.AreEqual(0, result.closebuttons);
            Assert.AreEqual("behold_title", result.top.symbol);

            Assert.AreEqual("vulcan_executioner_crew", result.crew1.symbol);
            Assert.AreEqual(0, result.crew1.stars);

            Assert.AreEqual("janeway_assimilated_crew", result.crew2.symbol);
            Assert.AreEqual(1, result.crew2.stars);

            Assert.AreEqual("guinan_gloria_crew", result.crew3.symbol);
            Assert.AreEqual(0, result.crew3.stars);

            result = searcher.Search(Path.Combine(Directory.GetCurrentDirectory(), "../../../fixtures/invalid.png"));
            Assert.IsFalse(result.IsValid());
        }

        [TestMethod]
        public void TestVoyImageParser()
        {
            VoyImage voyImage = new VoyImage(Path.Combine(Directory.GetCurrentDirectory(), "../../../../.."));
            Assert.AreNotEqual(null, voyImage);

            var result = voyImage.SearchImage(Path.Combine(Directory.GetCurrentDirectory(), "../../../fixtures/voy1.png"));
            Assert.IsTrue(result.valid);
            Assert.AreEqual(2750, result.antimatter);
            Assert.AreEqual(5296, result.cmd.SkillValue);
            Assert.AreEqual(0, result.cmd.Primary);
            Assert.AreEqual(3725, result.dip.SkillValue);
            Assert.AreEqual(0, result.dip.Primary);
            Assert.AreEqual(8869, result.eng.SkillValue);
            Assert.AreEqual(2, result.eng.Primary);
            Assert.AreEqual(9369, result.sec.SkillValue);
            Assert.AreEqual(1, result.sec.Primary);
            Assert.AreEqual(3037, result.med.SkillValue);
            Assert.AreEqual(0, result.med.Primary);
            Assert.AreEqual(5210, result.sci.SkillValue);
            Assert.AreEqual(0, result.sci.Primary);

            result = voyImage.SearchImage(Path.Combine(Directory.GetCurrentDirectory(), "../../../fixtures/voy2.png"));
            Assert.IsTrue(result.valid);
            Assert.AreEqual(2750, result.antimatter);
            Assert.AreEqual(5112, result.cmd.SkillValue);
            Assert.AreEqual(0, result.cmd.Primary);
            Assert.AreEqual(8783, result.dip.SkillValue);
            Assert.AreEqual(0, result.dip.Primary);
            Assert.AreEqual(4289, result.eng.SkillValue);
            Assert.AreEqual(0, result.eng.Primary);
            Assert.AreEqual(11005, result.sec.SkillValue);
            Assert.AreEqual(2, result.sec.Primary);
            Assert.AreEqual(11068, result.med.SkillValue);
            Assert.AreEqual(1, result.med.Primary);
            Assert.AreEqual(4797, result.sci.SkillValue);
            Assert.AreEqual(0, result.sci.Primary);

            result = voyImage.SearchImage(Path.Combine(Directory.GetCurrentDirectory(), "../../../fixtures/invalid.png"));
            Assert.IsFalse(result.valid);
        }
    }
}
