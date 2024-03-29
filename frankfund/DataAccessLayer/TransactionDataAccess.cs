﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.BigQuery.V2;
using DataAccessLayer.Models;

namespace DataAccessLayer
{
    public class TransactionDataAccess: DataAccess
    {
        private readonly DataHelper dataHelper;
        private readonly string tableID;
        private readonly string accTable;

        public TransactionDataAccess()
        {
            this.dataHelper = new DataHelper();
            this.tableID = this.dataHelper.getQualifiedTableName("Transactions");
            this.accTable = this.dataHelper.getQualifiedTableName("Accounts");
        }

        public BigQueryResults getUsingID(long ID)
        {
            string query = $"SELECT * from {this.tableID} WHERE TID = {ID}";
            return this.dataHelper.query(query, parameters: null);
        }

        // Write a transaction into BigQuery
        public void write(string[] serializedTransaction)
        {
            string query = $"INSERT INTO {this.tableID} VALUES ("
                + serializedTransaction[0] + ","                        // TransactionID
                + serializedTransaction[1] + ","                        // AccountID
                + serializedTransaction[2] + ","                        // SGID
                + $"\"{serializedTransaction[3]}\","                    // Transaction Name
                + serializedTransaction[4] + ","                        // Amount of transaction
                + $"\"{serializedTransaction[5]}\","                    // DateTime Transaction was made
                + $"\"{serializedTransaction[6]}\","                    // DateTime Transaction entered
                + serializedTransaction[7] + ","                        // Expense or Income
                + $"\"{serializedTransaction[8]}\")";                   // Transaction Category
            //Console.WriteLine("Running Insert Query:\n---------------------\n" + query);
            this.dataHelper.query(query);
        }

        // Delete an existing transaction with the given PK identifier
        public void delete(long TID)
        {
            string query = $"DELETE FROM {this.tableID} WHERE TID = {TID}";
            //Console.WriteLine("Transaction with TID " + TID + " was changed, updating records");
            this.dataHelper.query(query);
        }

        // Write an existing Transaction that changed during runtime
        public void update(string[] serializedTransaction)
        {
            // Delete the existing records for this transaction
            delete(long.Parse(serializedTransaction[0]));

            // Write the updated records
            write(serializedTransaction);
        }

        public long getNextAvailID()
        {
            return this.dataHelper.getNextAvailID(this.tableID);
        }

        // Overload wrappers to cast BigQuery Numeric type to C# decimal type
        public decimal castBQNumeric(BigQueryNumeric val)
        {
            return this.dataHelper.castBQNumeric(val);
        }
        public decimal castBQNumeric(object val)
        {
            return this.dataHelper.castBQNumeric(val);
        }
        public T ParseEnum<T>(string value)
        {
            return this.dataHelper.ParseEnum<T>(value);
        }


        // --------------------------------------  Transaction From Account --------------------------------------


        // Returns all transactions with a given user ordered by date entered
        public BigQueryResults getTransactionsFromAccount(long accID)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID}"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults getTransactionsFromAccount(string username)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}'"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // Returns all transactions from a user accross all categories, sorted by the category attribute
        public BigQueryResults getAllTransactionsCategorySorted(string username) {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}'"
                + " ORDER BY t.TransactionCategory DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults getAllTransactionsCategorySorted(long accID)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID}"
                + " ORDER BY t.TransactionCategory DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // Returns all transactions from a user account given a category ordered by date entered
        public BigQueryResults getTransactionsFromCategory(long accID, string category)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID} AND t.transactionCategory = \"{category}\""
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults getTransactionsFromCategory(string username, string category)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}' AND t.TransactionCategory = '{category}'"
                + " ORDER BY DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // --------------------------------------  Transaction From Account Time Period Sorting --------------------------------------

        // Returns all transactions from a user account made in the past n days
        public BigQueryResults SortTransactionsByDays(long accID, int num)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID} AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} DAY)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults SortTransactionsByDays(string username, int num)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}' AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} DAY)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // Returns all transactions from a user account made in the past n weeks
        public BigQueryResults SortTransactionsByWeeks(long accID, int num)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID} AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} WEEK)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults SortTransactionsByWeeks(string username, int num)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}' AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} WEEK)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // Returns all transactions from a user account made in the past n weeks
        public BigQueryResults SortTransactionsByMonths(long accID, int num)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID} AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} MONTH)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults SortTransactionsByMonths(string username, int num)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}' AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} MONTH)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }


        // Returns all transactions from a user account made in the past n years
        public BigQueryResults SortTransactionsByYear(long accID, int num)
        {
            string query = "SELECT * FROM FrankFund.Transactions t"
                + $" WHERE t.accountID = {accID} AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} YEAR)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
        public BigQueryResults SortTransactionsByYear(string username, int num)
        {
            string query = "SELECT t.TID, t.AccountID, t.SGID, t.TransactionName, t.Amount, t.DateTransactionMade"
                + ", t.DateTransactionEntered, t.IsExpense, t.TransactionCategory"
                + $" FROM {this.tableID} t"
                + $" INNER JOIN {this.accTable} a"
                + " ON t.AccountID = a.AccountID"
                + $" WHERE a.AccountUsername = '{username}' AND DateTransactionMade >= DATE_SUB(CURRENT_DATE(), INTERVAL {num} YEAR)"
                + " ORDER BY DateTransactionMade DESC, DateTransactionEntered DESC";
            return this.dataHelper.query(query, parameters: null);
        }
    }
}
