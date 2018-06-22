﻿﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using NUnit.Framework;
using QuantConnect.Securities.CurrencyConversion;
using QuantConnect.Securities.CurrencyConversion.PathProvider;
using System.Linq;

namespace QuantConnect.Tests.Common.Securities.CurrencyConversion.Graph
{
    [TestFixture]
    public class GraphSearchTests
    {
        [TestCase("BTC", "USD")]
        [TestCase("BTC", "EUR")]
        [TestCase("REN", "EUR")]
        [TestCase("REN", "ZRX")]
        [TestCase("BTC", "XAU")]
        [TestCase("REN", "XAU")]
        public void CurrencyConversionPathTest(string start, string end)
        {
            var pairs = Currencies.CurrencyPairs
                .Concat(Currencies.CryptoCurrencyPairs)
                .Concat(Currencies.CfdCurrencyPairs)
                .ToList();

            ICurrencyPathProvider graphSearch = new GraphSearchProvider();

            // type of security here doesn't matter, we just want to test shortest path search
            pairs.ForEach(pair => graphSearch.AddEdge(pair, SecurityType.Base));

            graphSearch.FindShortestPath(start, end);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "ETH")]
        public void GraphThrowsOnMissingStartCode()
        {
            ICurrencyPathProvider graph = new GraphSearchProvider();

            // add one test pair
            graph.AddEdge("ZRX", "BTC", SecurityType.Crypto);

            graph.FindShortestPath("ETH", "ZRX");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), MatchType = MessageMatch.Contains, ExpectedMessage = "ETH")]
        public void GraphThrowsOnMissingEndCode()
        {
            ICurrencyPathProvider graph = new GraphSearchProvider();

            // add one test pair
            graph.AddEdge("ZRX", "BTC", SecurityType.Crypto);

            graph.FindShortestPath("ZRX", "ETH");
        }

        [Test]
        public void SimpleCurrencyPathStepsCorrect()
        {
            ICurrencyPathProvider graph = new GraphSearchProvider();

            graph.AddEdge("ZRXBTC", SecurityType.Crypto); // inverted = false
            graph.AddEdge("ETHBTC", SecurityType.Crypto); // inverted = true
            graph.AddEdge("ETHREN", SecurityType.Crypto); // inverted = false
            graph.AddEdge("USDREN", SecurityType.Crypto); // inverted = true

            var path = graph.FindShortestPath("ZRX", "USD");

            var step = path.Steps.ToArray();

            Assert.AreEqual(step[0].Inverted, false);
            Assert.AreEqual(step[1].Inverted, true);
            Assert.AreEqual(step[2].Inverted, false);
            Assert.AreEqual(step[3].Inverted, true);
        }


        [Test] // longer path, and also test bidirectional pairs tests
        public void ComplicatedCurrencyPathStepsCorrect()
        {
            ICurrencyPathProvider graph = new GraphSearchProvider();

            graph.AddEdge("ZRXBTC", SecurityType.Crypto); // inverted = false
            graph.AddEdge("ETHBTC", SecurityType.Crypto); // inverted = true
            graph.AddEdge("ETHREN", SecurityType.Crypto); // inverted = false
            graph.AddEdge("USDREN", SecurityType.Crypto); // inverted = true

            // bidirectional
            graph.AddEdge("EURUSD", SecurityType.Crypto); // inverted = false
            graph.AddEdge("USDEUR", SecurityType.Crypto);

            graph.AddEdge("EURKRW", SecurityType.Crypto); // inverted = false;

            // add some unrelated pairs
            graph.AddEdge("ZRXMXN", SecurityType.Crypto);
            graph.AddEdge("MXNRUB", SecurityType.Crypto);
            graph.AddEdge("RUBLSK", SecurityType.Crypto);

            var step = graph.FindShortestPath("ZRX", "KRW").Steps.ToArray();

            Assert.AreEqual(step[0].Inverted, false);
            Assert.AreEqual(step[1].Inverted, true);
            Assert.AreEqual(step[2].Inverted, false);
            Assert.AreEqual(step[3].Inverted, true);
            Assert.AreEqual(step[4].Inverted, false);
            Assert.AreEqual(step[5].Inverted, false);
        }
    }
}
