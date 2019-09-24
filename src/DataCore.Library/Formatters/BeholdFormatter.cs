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
using System.Linq;
using System.Collections.Generic;

namespace DataCore.Library
{
    public static class BeholdFormatter
    {
        public static CrewData GetBest(SearchResults result, CrewData[] crew, out string title)
        {
            var best = crew.OrderBy(c => c.bigbook_tier).ToList();

            var starBest = new List<CrewData>();
            if ((result.crew1.stars > 0) && (result.crew1.stars < crew[0].max_rarity)) starBest.Add(crew[0]);
            if ((result.crew2.stars > 0) && (result.crew2.stars < crew[1].max_rarity)) starBest.Add(crew[1]);
            if ((result.crew3.stars > 0) && (result.crew3.stars < crew[2].max_rarity)) starBest.Add(crew[2]);
            var theStarBest = starBest.OrderBy(c => c.bigbook_tier).ToList();

            if (best[0].bigbook_tier > 9)
            {
                if (theStarBest.Count > 0)
                {
                    title = $"All these options suck, so add a star to {theStarBest[0].name} I guess";
                }
                else
                {
                    title = $"All these options suck, pick {best[0].name} if you have room";
                }
            }
            else
            {
                if ((theStarBest.Count > 0) && (theStarBest[0].name != best[0].name))
                {
                    if (theStarBest[0].bigbook_tier > 9)
                    {
                        title = $"{best[0].name} is your best bet; star up the crappy {theStarBest[0].name} if you don't have any slots to spare";
                    }
                    else
                    {
                        title = $"Add a star to {theStarBest[0].name} or pick {best[0].name} if you have room";
                    }
                }
                else
                {
                    if (best[0].bigbook_tier == best[1].bigbook_tier)
                    {
                        title = $"Pick either {best[0].name} or {best[1].name}";
                    }
                    else
                    {
                        int stars = 0;
                        if (best[0].symbol == result.crew1.symbol)
                        {
                            stars = result.crew1.stars;
                        }
                        else if (best[0].symbol == result.crew2.symbol)
                        {
                            stars = result.crew2.stars;
                        }
                        else
                        {
                            stars = result.crew3.stars;
                        }

                        if (stars == best[0].max_rarity)
                        {
                            if (best[1].bigbook_tier < 8)
                            {
                                title = $"{best[1].name} is your best bet, unless you want to start another {best[0].name}";
                            }
                            else
                            {
                                title = $"It may be worth starting another {best[0].name}, pick {best[1].name} if you don't want dupes";
                            }
                        }
                        else
                        {
                            title = $"{best[0].name} is your best bet";
                        }
                    }
                }
            }

            return best[0];
        }
    }
}