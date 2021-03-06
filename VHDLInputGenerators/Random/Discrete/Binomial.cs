﻿//
//  Copyright (C) 2010-2014  Denis Gavrish
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Text;
using VHDLRuntime;

namespace VHDLInputGenerators.Random.Discrete
{
    /* ================================================================ 
    * Returns a binomial distributed integer between 0 and n inclusive. 
    * NOTE: use n > 0 and 0.0 < p < 1.0
    * ================================================================
    */
    public class Binomial : My_Random_Discrete_Base
    {
        private Bernoulli bernoulli;

        public static string Description =
            "Returns a binomial distributed integer between 0 and n inclusive. \n"
            + "NOTE: use n > 0 and 0.0 < p < 1.0";

        private long n;
        public long N
        {
            get
            {
                return n;
            }
            set
            {
                if (n < 0)
                    throw new Exception("Argument n must be  n > 0");
                else
                    n = value;
            }
        }
        private double p;
        public double P
        {
            get
            {
                return p;
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                    throw new Exception("Argument p must be  0.0 < p < 1.0");
                else
                {
                    p = value;
                    bernoulli.P = value;
                }
            }
        }

        public Binomial(long n, double p)
        {
            bernoulli = new Bernoulli(p);
            this.N = n;
            this.P = p;
        }

        public override long NextValue()
        {
            long i, x = 0;

            for (i = 0; i < n; i++)
                x += bernoulli.NextValue();
            return x;
        }
        public override string ToString()
        {
            return "Random_Discrete : Binomial";
        }
        public override StringBuilder StringVhdlRealization(KeyValuePair<string, TimeInterval> param)
        {
            throw new NotImplementedException();
        }
    }
}
