﻿using System;
using DataAccessLayer;
using Google.Cloud.BigQuery.V2;
using DataAccessLayer.Models;
using System.Collections.Generic;

namespace ServiceLayer
{
    public class TransactionService: Service<Transaction>
    {
        private readonly TransactionDataAccess TransactionDataAccess;

        public TransactionService()
        {
            this.TransactionDataAccess = new TransactionDataAccess();
        }

        public Transaction reinstantiate(BigQueryRow row)
        {
            long SGID = -1;                     // Nullable attribute
            if (row["SGID"] != null)
            {
                SGID = (long)row["SGID"];
            }

            return new Transaction(
                (long)row["TID"], (long)row["AccountID"], SGID,
                (string)row["TransactionName"],
                this.TransactionDataAccess.castBQNumeric(row["Amount"]),
                (DateTime)row["DateTransactionMade"],
                (DateTime)row["DateTransactionEntered"],
                (bool)row["IsExpense"],
                //(transactionCategory)row["TransactionCategory"]
                this.TransactionDataAccess.ParseEnum<transactionCategory>((string)row["TransactionCategory"])
            );
        }

        /* Retrieve a SavingsGoal from db with a given SGID
            Params: The SGID of the Savings Goal to retrieve
            Returns: A reinstantiated Savings Goal matching the SGID or null if non existant
        */
        public Transaction getUsingID(long TID)
        {
            long SGID = -1;                     // Nullable attribute
            Transaction transaction = null;
            foreach (BigQueryRow row in this.TransactionDataAccess.getUsingID(TID))
            {
                if(row["SGID"] != null)
                {
                    SGID = (long)row["SGID"];
                }
                transaction = reinstantiate(row);
            }
            return transaction;
        }

        /*
        Serialize a Transaction object into a String array
            Returns: A string array with each element in order of its column attribute (see Transaction DB schema)
        */
        public string[] serialize(Transaction t)
        {
            return new string[] {
                t.getTID().ToString(),
                t.getAccountID().ToString(),
                t.getSGID().ToString(),
                t.getTransactionName().ToString(),
                t.getAmount().ToString(),
                t.getDateTransactionMade().ToString("yyyy-MM-dd"),
                t.getDateTransactionEntered().ToString("yyyy-MM-dd"),
                t.getIsExpense().ToString().ToLower(),                  // BigQuery stores boolean as {True, False}, C# stores as {true, false}
                t.getTransactionCategory().ToString()
            };
        }

        /*
        Convert a Transaction object into JSON format
            Params: A Transaction object to convert
            Returns: The JSON string representation of the object
        */
        public string getJSON(Transaction t)
        {
            if (t == null)
            {
                return "{}";
            }
            string[] serialized = serialize(t);
            string jsonStr = "{"
                + $"\"TID\":{serialized[0]},"
                + $"\"AccountID\":{serialized[1]},"
                + $"\"SGID\":{serialized[2]},"
                + $"\"TransactionName\":\"" + serialized[3] + "\","
                + $"\"Amount\":{serialized[4]},"
                + $"\"DateTransactionMade\":\"" + serialized[5] + "\","
                + $"\"DateTransactionEntered\":\"" + serialized[6] + "\","
                + $"\"IsExpense\":{serialized[7]},"
                + $"\"TransactionCategory\":\"" + serialized[8] + "\""
            + "}";
            return jsonStr;
        }

        public string getJSON(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
            {
                return "{}";
            }
            string jsonStr = "{\"Transactions\":[";
            for (int i = 0; i < transactions.Count; i++)
            {
                if (i == transactions.Count - 1)
                {
                    jsonStr += getJSON(transactions[i]);
                }
                else
                {
                    jsonStr += (getJSON(transactions[i]) + ", ");
                }
            }

            return jsonStr + "]}";
        }


        // Delete a transaction with the given PK Identifier
        public void delete(long TID)
        {
            this.TransactionDataAccess.delete(TID);
        }

        // Serialize a NEWLY created Transaction runtime object and write it to BigQuery for the first time
        public void write(Transaction t)
        {
            string[] serializedTransaction = serialize(t);
            this.TransactionDataAccess.write(serializedTransaction);
        }


        // Serialize and update an EXISTING Transaction in BigQuery only if it CHANGED during runtime
        public void update(Transaction t)
        {
            if (t.changed)
            {
                string[] serializedTransaction = serialize(t);
                this.TransactionDataAccess.update(serializedTransaction);
            }
        }


        /* Wrapper method, query DB for next available TID
            Returns: Next available TID (1 + the maximum TID currently in the DB)
        */
        public long getNextAvailID()
        {
            return TransactionDataAccess.getNextAvailID();
        }

        /*
        Returns all transactions that is associated with the given account ordered by date entered
            Params: The User Account ID 
            Returns: A list of Transactions associated with the given ID
         */
        public List<Transaction> getTransactionsFromAccount(long accID)
        {
            List<Transaction> transactionsList = new List<Transaction>();

            foreach (BigQueryRow row in this.TransactionDataAccess.getTransactionsFromAccount(accID))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }
        public List<Transaction> getTransactionsFromAccount(string username)
        {
            List<Transaction> transactionsList = new List<Transaction>();

            foreach (BigQueryRow row in this.TransactionDataAccess.getTransactionsFromAccount(username))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }


        /*
        Returns all transactions associated with the given account and category.
            Params: The User Account ID
                    The category
            Returns: A list of Transactions associated with user account sorted by category
        */
        public List<Transaction> getTransactionsFromCategory(long accID, string category)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            foreach (BigQueryRow row in this.TransactionDataAccess.getTransactionsFromCategory(accID, category))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }
        public List<Transaction> getTransactionsFromCategory(string username, string category)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            foreach (BigQueryRow row in this.TransactionDataAccess.getTransactionsFromCategory(username, category))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }

        /*
        Returns all transactions associated with the given account and sorted by category (returns transactions from all categories).
            Params: The User Account ID
                    The category
            Returns: A list of Transactions associated with user account sorted by category
        */
        public List<Transaction> getAllTransactionsCategorySorted(string username)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            foreach (BigQueryRow row in this.TransactionDataAccess.getAllTransactionsCategorySorted(username))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }
        public List<Transaction> getAllTransactionsCategorySorted(long accID)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            foreach (BigQueryRow row in this.TransactionDataAccess.getAllTransactionsCategorySorted(accID))
            {
                Transaction transaction = reinstantiate(row);
                transactionsList.Add(transaction);
            }
            return transactionsList;
        }

        /*
        Returns all transactions associated with a user account that was made within X amount of time.
            Params: The User Account ID
                    The number of days, weeks, months, or years
                    The choice of sorting; 0 = days, 1 = weeks, 2 = months, 3 = years.
            Returns: A list of Transactions that are made within X amount of time.
        */
        public List<Transaction> getSortedTransactionsByTime(long accID, int num, int choice)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            switch (choice)
            {
                //day
                case 0:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByDays(accID, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //week
                case 1:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByWeeks(accID, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //month
                case 2:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByMonths(accID, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //year
                case 3:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByYear(accID, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
            }
            return transactionsList;
        }
        public List<Transaction> getSortedTransactionsByTime(string username, int num, int choice)
        {
            List<Transaction> transactionsList = new List<Transaction>();
            switch (choice)
            {
                //day
                case 0:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByDays(username, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //week
                case 1:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByWeeks(username, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //month
                case 2:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByMonths(username, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
                //year
                case 3:
                    foreach (BigQueryRow row in this.TransactionDataAccess.SortTransactionsByYear(username, num))
                    {
                        Transaction transaction = reinstantiate(row);
                        transactionsList.Add(transaction);
                    }
                    break;
            }
            return transactionsList;
        }



        /* Cast a category string to transactionCategory enum
            Params: The category string to cast
            Returns: The corresponding enum transactionCategory
        */
        public transactionCategory castCategory(string c)
        {
            if (c.Equals("Entertainment"))
                return transactionCategory.Entertainment;
            else if (c.Equals("Restaurants"))
                return transactionCategory.Restaurants;
            else if (c.Equals("Transportation"))
                return transactionCategory.Transportation;
            else if (c.Equals("HomeAndUtilities"))
                return transactionCategory.HomeAndUtilities;
            else if (c.Equals("Education"))
                return transactionCategory.Education;
            else if (c.Equals("Insurance"))
                return transactionCategory.Insurance;
            else if (c.Equals("Health"))
                return transactionCategory.Health;
            else if (c.Equals("Groceries"))
                return transactionCategory.Groceries;
            else if (c.Equals("Deposits"))
                return transactionCategory.Deposits;
            else if (c.Equals("Shopping"))
                return transactionCategory.Shopping;
            else
                return transactionCategory.Uncategorized;

        }
    }
}
