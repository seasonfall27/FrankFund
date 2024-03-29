﻿using System;
using DataAccessLayer;
using Google.Cloud.BigQuery.V2;
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLayer
{
    public class AnalyticsService
    {
        private readonly AnalyticsDataAccess dataAccess;
        private readonly Dictionary<int, string> monthMap;
        private readonly List<Tuple<string, decimal>> monthSpendingTemplate;
        private readonly Dictionary<string, decimal> categorySpendingTemplate;
        private readonly TransactionService ts;
        private readonly int endMonth;

        public AnalyticsService()
        {
            this.dataAccess = new AnalyticsDataAccess();
            this.ts = new TransactionService();
            this.monthMap = new Dictionary<int, string>
            {
                {1, "Jan"},   {2, "Feb"},   {3, "Mar"},   {4, "Apr"},
                {5, "May" },  {6, "Jun"},   {7, "Jul"},   {8, "Aug"},
                {9, "Sep"},   {10, "Oct"},  {11, "Nov"},  {12, "Dec"}
            };


            // Generate template mapping of month where months are ordered as:
            // Analytics over the past year should be run up until last month
            // (today's current month is not included because it is not over yet)
            endMonth = (int)DateTime.Now.Month - 1;
            if (endMonth < 1)
                endMonth = 12;

            this.monthSpendingTemplate = new List<Tuple<string, decimal>>();
            if(endMonth == 12)
            {
                for(int i = 1; i < 13; i++)
                    monthSpendingTemplate.Add(Tuple.Create(this.monthMap[i], (decimal) -1.0));
            }
            // All months up until the end of last year, then all of the months this year up until the end month
            else
            {
                for(int i = endMonth + 1; i < 13; i++)
                    monthSpendingTemplate.Add(Tuple.Create(this.monthMap[i], (decimal) 0));
                for(int i = 1; i <= endMonth;i++)
                    monthSpendingTemplate.Add(Tuple.Create(this.monthMap[i], (decimal) 0));
            }

            this.categorySpendingTemplate = new Dictionary<string, decimal>
            {
                { "Entertainment", (decimal) 0 },       { "Restaurants", (decimal) 0 },     { "Transportation", (decimal) 0 },
                { "HomeAndUtilities", (decimal) 0 },    { "Education", (decimal) 0 },       { "Insurance", (decimal) 0 },
                { "Health", (decimal) 0 },              { "Groceries", (decimal) 0 },       { "Deposits", (decimal) 0 },
                { "Shopping", (decimal) 0 },            { "Uncategorized", (decimal) 0 }
            };
        }

        public void printTuples()
        {
            Console.WriteLine("End is: " + this.endMonth);
            foreach (Tuple<string, decimal> t in this.monthSpendingTemplate)
            {
                Console.WriteLine(t.Item1);
            }
            Console.WriteLine(getJSON(this.monthSpendingTemplate));
        }


        // Generate JSON string of a list where each element maps month string to decimal value for that month
        public string getJSON(List<Tuple<string, decimal>> vals) 
        {
            if (vals == null || vals.Count == 0)
            {
                return "{}";
            }
            string jsonStr = "{\"MonthVals\":[";
            for (int i = 0; i < vals.Count; i++)
            {
                jsonStr += $"{{\"month\":\"{vals[i].Item1}\", \"amt\":{vals[i].Item2}}}, ";
            }
            jsonStr += $"{{\"month\":\"{this.monthMap[(int)DateTime.Now.Month]}\", \"amt\":null}}]}}";

            return jsonStr;
        }

        public string getJSON(Dictionary<string, decimal> categoryValues) {
            if (categoryValues == null || categoryValues.Count == 0)
            {
                return "{}";
            }
            string jsonStr = "{";
            int i = 0;
            foreach(var pair in categoryValues)
            {
                if (i == categoryValues.Count - 1)
                {
                    jsonStr += $"\"{pair.Key}\":{pair.Value}";
                }
                else
                {
                    jsonStr += $"\"{pair.Key}\":{pair.Value}, ";
                }
                i++;
            }
            return jsonStr + "}";
        }

        public string getTopPctsJSON(List<Tuple<string, decimal>> vals)
        {
            if (vals == null || vals.Count == 0)
            {
                return "{}";
            }
            string jsonStr = "{\"CategoryPcts\":[";
            for (int i = 0; i < vals.Count; i++)
            {
                if (i == vals.Count - 1)
                {
                    jsonStr += $"{{\"category\":\"{vals[i].Item1}\", \"pct\":{vals[i].Item2}}}";
                }
                else
                {
                    jsonStr += $"{{\"category\":\"{vals[i].Item1}\", \"pct\":{vals[i].Item2}}}, ";
                }
            }

            return jsonStr + "]}";
        }


        // Perform a deep copy of the month spending template (to copy the ordering)
        public List<Tuple<string, decimal>> getTemplate(decimal defaultVal)
        {
            List<Tuple<string, decimal>> copy = new List<Tuple<string, decimal>>();
            foreach (Tuple<string, decimal> t in this.monthSpendingTemplate)
                copy.Add(Tuple.Create(t.Item1, defaultVal));
            return copy;
        }
        // Perform a deep copy of the category to spending templateßßßß
        public Dictionary<string, decimal> getCategoryTemplate()
        {
            Dictionary<string, decimal> copy = new Dictionary<string, decimal>();
            foreach (var pair in this.categorySpendingTemplate)
                copy.Add(pair.Key, pair.Value);
            return copy;
        }


        public List<Tuple<string, decimal>> getSpendingPerMonthPastYear(string username) {
            List<Tuple<string, decimal>> monthSpending = getTemplate(0);
            string currMonth;
            decimal currTotal;

            foreach(BigQueryRow r in this.dataAccess.getSpendingPerMonthPastYear(username))
            {
                currTotal = dataAccess.castBQNumeric(r["Total"]);
                currMonth = this.monthMap[Convert.ToInt32(r["Month"])];
                for(int i = 0; i < monthSpending.Count; i++)
                {
                    if (monthSpending[i].Item1.Equals(currMonth))
                        monthSpending[i] = Tuple.Create(currMonth, currTotal);
                }
            }
            return monthSpending;
        }

        public List<Tuple<string, decimal>> getSavingsPerMonthPastYear(string username)
        {
            List<Tuple<string, decimal>> monthSavings= getTemplate(0);
            string currMonth;
            decimal currTotal;

            foreach (BigQueryRow r in this.dataAccess.getSavingsPerMonthPastYear(username))
            {
                currTotal = dataAccess.castBQNumeric(r["Total"]);
                currMonth = this.monthMap[Convert.ToInt32(r["Month"])];
                for (int i = 0; i < monthSavings.Count; i++)
                {
                    if (monthSavings[i].Item1.Equals(currMonth))
                        monthSavings[i] = Tuple.Create(currMonth, currTotal);
                }
            }
            return monthSavings;
        }

        public List<Tuple<string, decimal>> getExpenseRatioPastYear(string username)
        {
            var monthSavings = getSavingsPerMonthPastYear(username);
            var monthSpending = getSpendingPerMonthPastYear(username);
            List<Tuple<string, decimal>> monthRatio = getTemplate(1);
            foreach(var pair in monthRatio)
            {
                Console.WriteLine(pair.Item1, pair.Item2);
            }
            Tuple<string, decimal> currSavingsPair, currSpendingPair;
            string currMonth;
            decimal currMonthRatio, currMonthSpending, currMonthSavings;

            // Calculate and store savings to spending ratio for each month
            // Value >= 1 means user saves as much or more than they spend for that month
            for (int i = 0; i < monthSavings.Count; i++)
            {
                currSavingsPair = monthSavings[i];
                currSpendingPair = monthSpending[i];
                currMonthSavings = currSavingsPair.Item2 == 0 ? 1 : currSavingsPair.Item2;
                currMonthSpending = currSpendingPair.Item2 == 0 ? 1 : currSpendingPair.Item2;
            
                currMonth = currSavingsPair.Item1;
                currMonthRatio = Math.Round(currMonthSavings / currMonthSpending, 2);
                monthRatio[i] = Tuple.Create(currMonth, currMonthRatio);
            }
            return monthRatio;
        }

        

        // Given an account ID, return a dictionary that maps spending category to the sum of
        // expenses in that category for a particular user
        // Returns null if account not found
        public Dictionary<string,decimal> getAllTimeSpendingPerCategory(long accID)
        {
            Dictionary<string, decimal> spendingPerCategory = getCategoryTemplate();
            foreach (BigQueryRow row in dataAccess.getAllTimeSpendingPerCategory(accID))
                spendingPerCategory[Convert.ToString(row["TransactionCategory"])] = dataAccess.castBQNumeric(row["CategoryTotal"]);

            // No acount exists with the given accID
            if(spendingPerCategory.Count == 0)
            {
                return null;
            }
            // Otherwise return the breakdown
            else
            {
                return spendingPerCategory;
            }
        }
        public Dictionary<string, decimal> getAllTimeSpendingPerCategory(string username)
        {
            Dictionary<string, decimal> spendingPerCategory = getCategoryTemplate();
            foreach (BigQueryRow row in dataAccess.getAllTimeSpendingPerCategory(username))
                spendingPerCategory[Convert.ToString(row["TransactionCategory"])] = dataAccess.castBQNumeric(row["CategoryTotal"]);

            // No acount exists with the given accID
            if (spendingPerCategory.Count == 0)
            {
                return null;
            }
            // Otherwise return the breakdown
            else
            {
                return spendingPerCategory;
            }
        }



        // Given an account ID, return the sum of all transactions across ALL categories
        // Returns 0 if account not found
        public decimal getTotalSpending(long accID)
        {
            decimal totalSpending = 0;
            foreach (BigQueryRow row in dataAccess.getTotalSpending(accID))
            {
                totalSpending = dataAccess.castBQNumeric(row["TotalExpenses"]);
            }
            return totalSpending;
        }
        public decimal getTotalSpending(string username)
        {
            decimal totalSpending = 0;
            foreach (BigQueryRow row in dataAccess.getTotalSpending(username))
            {
                totalSpending = dataAccess.castBQNumeric(row["TotalExpenses"]);
            }
            return totalSpending;
        }


        public decimal getTotalSavings(string username)
        {
            decimal totalSavings = 0;
            foreach (BigQueryRow row in dataAccess.getTotalSavings(username))
            {
                totalSavings = dataAccess.castBQNumeric(row["TotalSavings"]);
            }
            return totalSavings;
        }
        public decimal getTotalSavingsThisYear(string username)
        {
            decimal totalSavings = 0;
            foreach (BigQueryRow row in dataAccess.getTotalSavingsThisYear(username))
            {
                totalSavings = dataAccess.castBQNumeric(row["TotalSavings"]);
            }
            return totalSavings;
        }

        public Transaction getMostExpensiveTransaction(long accID)
        {
            foreach(BigQueryRow row in dataAccess.getMostExpensiveTransaction(accID))
            {
                return ts.reinstantiate(row);
            }
            return null;
        }



        // Given an account ID, return a breakdown of the spending per category
        // Returns a dictionary that maps the category to a tuple of spending in the category, and percentage of total spending
        // Returns null if account does not exist
        //public Dictionary<string, Tuple<decimal, decimal>> getAllTimeCategoryBreakdown(long accID)
        //{
        //    decimal totalSpending = getTotalSpending(accID);
        //    Dictionary<string, Tuple<decimal, decimal>> categoryBreakdown = new Dictionary<string, Tuple<decimal, decimal>>();
        //    Dictionary<string, decimal> spendingPerCategory = getAllTimeSpendingPerCategory(accID);
        //    if(spendingPerCategory == null || totalSpending == -1)
        //    {
        //        return null;
        //    }

        //    decimal currCategoryTotal, currPercentage;
        //    foreach (string category in spendingPerCategory.Keys)
        //    {
        //        currCategoryTotal = spendingPerCategory[category];
        //        currPercentage = Math.Round(currCategoryTotal / totalSpending, 2);
        //        categoryBreakdown.Add(category, new Tuple<decimal, decimal>(currCategoryTotal, currPercentage));
        //    }

        //    if(categoryBreakdown.Count == 0)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return categoryBreakdown;
        //    }
        //}


        public Dictionary<string, decimal> getAllTimeCategoryBreakdown(string username)
        {
            decimal totalSpending = getTotalSpending(username);
            if(totalSpending == 0)
            {
                return new Dictionary<string, decimal>();
            }
            Dictionary<string, decimal> categoryBreakdown = getCategoryTemplate();
            Dictionary<string, decimal> spendingPerCategory = getAllTimeSpendingPerCategory(username);
            if (spendingPerCategory == null || totalSpending == -1)
            {
                return null;
            }

            decimal currCategoryTotal, currPercentage;
            foreach (string category in spendingPerCategory.Keys)
            {
                currCategoryTotal = spendingPerCategory[category];
                currPercentage = Math.Round((currCategoryTotal / totalSpending) * 100, 2);
                categoryBreakdown[category] = currPercentage;
            }
            return categoryBreakdown.Count == 0 ? null : categoryBreakdown;
        }


        // Over all time
        public List<Tuple<string, decimal>> getTopSpendingCategoryPercentages(string username)
        {
            // Extract top 3 categories
            List<Tuple<string, decimal>> categoryPercentagesSorted = new List<Tuple<string, decimal>>();
            foreach (KeyValuePair<string, decimal> kvp in getAllTimeCategoryBreakdown(username).OrderByDescending(key => key.Value))
            {
                if (categoryPercentagesSorted.Count == 3)
                    break;
                if (kvp.Key.Equals("Deposits"))
                    continue;
                categoryPercentagesSorted.Add(Tuple.Create(kvp.Key, kvp.Value));
            }

            return categoryPercentagesSorted;

        }

    }
}