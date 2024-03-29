using System;
using System.IO;
using ServiceLayer;
using DataAccessLayer.Models;
using System.Collections.Generic;
using DataAccessLayer;


namespace ServiceLayer
{
    class debug
    {
        static void authenticateGCP()
        {
            // Devin's Credentials
            string pathToCreds = "../../Credentials/AuthDevin.json";

            // Autumn's Credentials
            //string pathToCreds = "/Users/steve/OneDrive/Documents/GitHub/FrankFund/Credentials/AuthAutumn.json";

            // Kenneth's Credentials
            //string pathToCreds = "/Users/015909177/Desktop/Github Repos/FrankFund/Credentials/AuthKenny.json";

            //Rachel's Credentials 
            //string pathToCreds = "C:/Data/Spring 2021/CECS 491B/Senior Project/Credentials/AuthRachel.json";

            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToCreds);
        }

        // --------------------------------------------------- Begin Savings Goal Testing ---------------------------------------------------
        static SavingsGoal testSGCreateByDate()
        {
            Console.WriteLine("\n-------------------- Test: Creating A New Savings Goal By End Date, Write --------------------");

            // Query DB for the next avail ID
            SavingsGoalService sgService = new SavingsGoalService();
            long SGID = sgService.getNextAvailID();
            Console.WriteLine("Next available SGID that can be assigned: " + SGID + "\n");

            // Create a new Savings Goal for the amount of $150 ending on December 20th, dynamically calculate payments
            DateTime endDate = new DateTime(2021, 12, 20, 0, 0, 0).Date;
            SavingsGoal sampleGoal = new SavingsGoal(SGID, 2, "Christmas Gift", (decimal)150.00, contrPeriod.Monthly, endDate);

            // Print summary of goal that was just created
            Console.WriteLine("Savings Goal Summary:\n---------------------");
            Console.WriteLine(sampleGoal);

            // Write SavingsGoal object to DB
            sgService.write(sampleGoal);

            return sampleGoal;
        }

        static SavingsGoal testSGCreateByContrAmt()
        {
            Console.WriteLine("\n\n-------------------- Test: Creating A New Savings Goal By Contribution Amount, Write --------------------");

            // Query DB for the next avail ID
            SavingsGoalService sgService = new SavingsGoalService();
            long SGID = sgService.getNextAvailID();
            Console.WriteLine("Next available SGID that can be assigned: " + SGID + "\n");

            // Create a new Savings Goal for the amount of $300 dynamically calculate end date
            DateTime endDate = new DateTime(2021, 12, 20, 0, 0, 0).Date;
            SavingsGoal sampleGoal = new SavingsGoal(SGID, 2, "Tuition", (decimal)3425.00, contrPeriod.Weekly, (decimal)325.00);

            // Print summary of goal that was just created
            Console.WriteLine("Savings Goal Summary:\n---------------------");
            Console.WriteLine(sampleGoal);

            // Write SavingsGoal object to DB
            sgService.write(sampleGoal);

            return sampleGoal;
        }

        static void testSGShowSerialize(SavingsGoal sg)
        {
            Console.WriteLine("\n\n-------------------- Test: Display String Serialization of SavingsGoal Object --------------------");

            // Serialize the runtime object to String[]
            SavingsGoalService sgService = new SavingsGoalService();
            string[] serialized = sgService.serialize(sg);
            Console.WriteLine("\nSavingsGoal Serialized:\n-----------------------\n");

            // Print the serialized SavingsGoal
            foreach (string attr in serialized)
            {
                Console.WriteLine("   " + attr);
            }
        }

        static void testSGShowJSON(SavingsGoal sg)
        {
            Console.WriteLine("\n\n-------------------- Test: Display JSON of SavingsGoal Object --------------------");

            // Print the JSON representation of the goal we created
            SavingsGoalService sgService = new SavingsGoalService();
            Console.WriteLine(sgService.getJSON(sg));
        }

        static void testSGReadModifyRewrite(long SGID)
        {
            Console.WriteLine("\n\n-------------------- Test: Recreate, Modify, Rewrite Existing SavingsGoal --------------------");

            // Reinstantiate a SavingsGoal from DB records
            SavingsGoalService sgService = new SavingsGoalService();
            SavingsGoal existingGoal = sgService.getUsingID(SGID);

            // Print summary of goal that was just reinstantiated
            Console.WriteLine("Savings Goal Summary:\n---------------------");
            Console.WriteLine(existingGoal + "\n");

            // Modify the payment period, system recalculates end date, redisplay summary
            existingGoal.updatePeriod(contrPeriod.Weekly);
            Console.WriteLine("Savings Goal Summary:\n---------------------");
            Console.WriteLine(existingGoal + "\n");

            // Modify the payment amound to half, system recalculates number of payment periods needed and end date, redisplay summary
            existingGoal.updateContrAmt(existingGoal.contrAmt * 2);
            Console.WriteLine("Savings Goal Summary:\n---------------------");
            Console.WriteLine(existingGoal + "\n");

            // Rewrite the modified goal to DB
            sgService.update(existingGoal);
        }

        static void testGetAllSGFromAcc(long accID)
        {
            Console.WriteLine("\n\n-------------------- Test: Retrieve All Savings Goals From Account --------------------");
            SavingsGoalService sgService = new SavingsGoalService();
            List<SavingsGoal> goals = sgService.getSavingsGoalsFromAccount(accID);
            foreach(SavingsGoal goal in goals)
            {
                Console.WriteLine(goal);
            }
        }

        static void testJSONGoalsList(long accID)
        {
            Console.WriteLine("\n\n-------------------- Test: JSONify a List of Savings Goals--------------------");
            //SavingsGoalService sgService = new SavingsGoalService();
            //List<SavingsGoal> goals = sgService.getSavingsGoalsFromAccount(accID);
            //string json = sgService.getJSON(goals);
            //Console.WriteLine(json);

            UserAccountService uac = new UserAccountService();
            Console.WriteLine(uac.getSavingsGoalsFromAccount(accID));
        }


        // --------------------------------------------------- Begin User Account Testing ---------------------------------------------------

        static void testAccCreate()
        {
            UserAccountService uaService = new UserAccountService();
            PasswordService passService = new PasswordService();

            Console.WriteLine("\n-------------------- Test: Getting and Creating a new User Account --------------------");
            //Console.WriteLine("\n1. Testing GetAccountUsingUsername function | Prints out AccountUsername found");
            //String existingAccount = uaService.getUsingUsername("AutumnNguyen").AccountUsername;
            //Console.WriteLine("Username: " + existingAccount);

            //Console.WriteLine("\n2. Testing GetAccountUsingID function | Prints out AccountUsername found");
            //String existingAccount2 = uaService.getUsingID(2).AccountUsername;
            //Console.WriteLine("Username: " + existingAccount2);

            //Console.WriteLine("\n3. Testing Password Minimum Requirements function");
            //UserAccount testAccount = new UserAccount("test", "test@gmail.com", "Password1");
            //uaService.write(testAccount);
            //Console.WriteLine(passService.checkHash("Password1", testAccount));

            //Console.WriteLine("Username: " + uaService.getUsingID(5).AccountUsername);

            //Console.WriteLine("\n4. Testing DeleteUserAccount function | Prints out AccountUsername of deleted account");
            //Console.WriteLine("Disable Username: " + uaService.getUsingID(5).AccountUsername);
            //uaService.delete(5);

            //Console.WriteLine("\n5. Testing CreateUserAccount function - New | Prints out AccountUsername of made account");
            //UserAccount testAccount2 = new UserAccount("test2", "test2@gmail.com", "password", null);
            //uaService.write(testAccount2);
            //Console.WriteLine("Username: " + uaService.getUsingUsername("test2").AccountUsername);

            //Console.WriteLine("\n6. Testing UpdateUserAccount function | Prints out new AccountUsername of account");
            //Console.WriteLine("Old Username: " + uaService.getUsingUsername("test").AccountUsername);
            //UserAccount testAccount3 = new UserAccount(5, "testing", "testing@gmail.com", "Password1");
            //uaService.update(testAccount3);
            //Console.WriteLine("New Username: " + uaService.getUsingUsername("testing").AccountUsername);

            //Console.WriteLine("\n7. Testing DeleteUserAccount function | Prints out AccountUsername of deleted account");
            //Console.WriteLine("Disable Username: " + uaService.getUsingUsername("testing").AccountUsername);
            //uaService.delete(5);

            Console.WriteLine("\n8. Password Validation");
            var user = uaService.getUsingUsername("test");
            Console.WriteLine("Username: " + user.AccountUsername);
            //Console.WriteLine("\nDoes the password match? " + passService.checkHash("Password1", user));
            Console.WriteLine("Does the password match?: " + passService.ValidatePassword("Password1", user.PasswordSalt, user.PasswordHash));
        }


        // --------------------------------------------------- Begin Transaction Testing ---------------------------------------------------

        static Transaction testAddTransaction()
        {
            Console.WriteLine("\n-------------------- Test: Creating A New Transaction, Write -------------------------");
            // Query DB for the next avail ID
            TransactionService ts = new TransactionService();
            long TID = ts.getNextAvailID();
            Console.WriteLine("Next available TID that can be assigned: " + TID + "\n");

            // Create a new Transaction
            //Temp Account ID
            long tempAccountID = 2;
            long SGID = 1;
            Transaction sampleTransaction = new Transaction(TID, tempAccountID, SGID, "Surgery", (decimal)5120.14, new DateTime(2020, 10, 07, 0, 0, 0).Date, true, transactionCategory.Health);

            // Print summary of goal that was just created
            Console.WriteLine("Transaction Summary:\n---------------------");
            Console.WriteLine(sampleTransaction);

            // Write SavingsGoal object to DB
            ts.write(sampleTransaction);

            return sampleTransaction;
        }

        static Transaction testReinstantiateTransaction(long TID)
        {
            TransactionService ts = new TransactionService();
            return ts.getUsingID(TID);
        }

        static void testModifyRewriteTransaction(long TID)
        {
            TransactionService ts = new TransactionService();
            Transaction t = ts.getUsingID(TID);
            t.setAmount(t.getAmount() * 2);
            ts.update(t);
        }

        static void testDeleteTransaction(long TID)
        {
            TransactionService ts = new TransactionService();
            ts.delete(TID);
        }

        static void testDisplayTransaction(long TID)
        {
            TransactionService ts = new TransactionService();
            Transaction t = ts.getUsingID(TID);
            Console.WriteLine("Retrieving Transaction with TID {0} from the database....", TID);
            Console.WriteLine(ts.getJSON(t));
            Console.WriteLine("Displaying summary of Transaction with TID {0}....", TID);
            Console.WriteLine(t);
        }

        // Displaying summary of transactions
        static void testGetTransactionsFromAccount(long accID)
        {
            List<Transaction> transactions = new List<Transaction>();
            TransactionService ts = new TransactionService();
            transactions = ts.getTransactionsFromAccount(accID);
            Console.WriteLine("\nDisplaying all transactions for user with account id {0}...", accID);
            foreach (Transaction t in transactions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(t);
            }
        }

        // Displaying each transactions in JSON format
        static void testGetTransactionsFromAccountJSON(long accID)
        {
            List<Transaction> transactions = new List<Transaction>();
            TransactionService ts = new TransactionService();
            transactions = ts.getTransactionsFromAccount(accID);
            Console.WriteLine("\nDisplaying all transactions for user with account id {0}...", accID);
            foreach (Transaction t in transactions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(ts.getJSON(t));
            }
        }

        //Displays summary of transactions by category
        static void testGetTransactionsFromCategory(long accID, string category)
        {
            List<Transaction> transactions = new List<Transaction>();
            TransactionService ts = new TransactionService();
            transactions = ts.getTransactionsFromCategory(accID, category);
            Console.WriteLine("\nDisplaying all transactions categorized as {0} for user with account id {1}...", accID, category);
            foreach (Transaction t in transactions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(t);
            }

        }
        //Displays transactions by category in JSON format
        static void testGetTransactionsFromCategoryJSON(long accID, string category)
        {
            List<Transaction> transactions = new List<Transaction>();
            TransactionService ts = new TransactionService();
            transactions = ts.getTransactionsFromCategory(accID, category);
            Console.WriteLine("\nDisplaying all transactions categorized as {0} for user with account id {1}...", accID, category);
            foreach (Transaction t in transactions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(ts.getJSON(t));
            }
        }

        static void testGetSortedTransactionsByTime(long accID, int num, int choice)
        {
            List<Transaction> transactions = new List<Transaction>();
            TransactionService ts = new TransactionService();
            transactions = ts.getSortedTransactionsByTime(accID, num, choice);
            string x;
            if (choice == 0)
            {
                x = "day(s).";
            }
            else if (choice == 1)
            {
                x = "week(s).";
            }
            else if (choice == 2)
            {
                x = "month(s).";
            }
            else
            {
                x = "year(s).";
            }
            Console.WriteLine("\nDisplaying all transactions for user with account id {0} in the last {1} {2}", accID, num, x);
            foreach(Transaction t in transactions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(t);
            }
        }

        // --------------------------------------------------- Begin Receipt Testing ---------------------------------------------------

        static Receipt testAddReceipt()
        {
            Console.WriteLine("\n-------------------- Test: Creating A New Receipt, Write -------------------------");
            // Query DB for the next avail ID
            ReceiptService rs = new ReceiptService();
            long RID = rs.getNextAvailID();
            Console.WriteLine("Next available RID that can be assigned: " + RID + "\n");

            //Create a new Receipt 
            long tempTransactionID = 1;
            string tempStringURL = "https://i.pinimg.com/736x/06/5e/86/065e86fe5eea5ff5ed21dff142eeba48.jpg";

            Receipt sampleReceipt = new Receipt(RID, tempTransactionID, tempStringURL, new DateTime(2021, 03, 16, 0, 0, 0), "short note", newlyCreated: true);

            //Print summary of Receipt that was just created
            Console.WriteLine("Receipt Summary: \n-----------------------");
            Console.WriteLine(sampleReceipt);
            rs.write(sampleReceipt);

            return sampleReceipt;
        }

        static void testSerializeReceipt(Receipt r)
        {
            ReceiptService rs = new ReceiptService();
            Console.WriteLine();
            foreach(string s in rs.serialize(r)) {
                Console.WriteLine(s);
            }
        }

        static void testReceiptJSON(Receipt r)
        {
            ReceiptService rs = new ReceiptService();
            Console.WriteLine("\n" + rs.getJSON(r));       
        }

        static Receipt testReinstantiateReceipt(long RID)
        {
            ReceiptService rs = new ReceiptService();
            Receipt r = rs.getUsingID(RID);
            return r;
        }

        static void testModifyRewriteReceipt(long RID) {
            ReceiptService rs = new ReceiptService();
            Receipt r = rs.getUsingID(10);
            Console.WriteLine(r);
            r.setNote("This purchase is for a purchase!");
            Console.WriteLine("\nAfter change: \n" + r);
            rs.update(r);
        }

        static void testDeleteReceipt(long RID)
        {
            ReceiptService rs = new ReceiptService();
            rs.delete(RID);
        }

        static void testUploadReceipt(string userName, string localPathToFile, string fileName)
        {
            ReceiptService rs = new ReceiptService();
            string url = rs.uploadFile(userName, localPathToFile, fileName);
            Console.WriteLine($"Uploaded {fileName} to cloud storage at: " + url);
        }

        static void testDownloadReceipt(string userName, string fileName, string dstPath)
        {
            ReceiptService rs = new ReceiptService();
            string downloadPath = rs.downloadFile(userName, fileName, dstPath);
            Console.WriteLine($"File {fileName} was downloaded from cloud storage to: {downloadPath}");
        }



        // --------------------------------------------------- Begin Data Analytics Testing ---------------------------------------------------
        //static void testSpendingPerCategory(long accID)
        //{
        //    AnalyticsService a = new AnalyticsService();

        //    Dictionary<string, decimal> spendingPerCategory = a.getAllTimeSpendingPerCategory(accID);

        //    // Print the spending per category
        //    Console.WriteLine("For user #" + accID);
        //    Console.WriteLine("\nSpending Per Category\n---------------------\n");
        //    foreach(string category in spendingPerCategory.Keys)
        //    {
        //        Console.WriteLine("   " + category + " : $" + spendingPerCategory[category]);
        //    }
        //}

        //static void testGetTotalSpending(long accID)
        //{
        //    AnalyticsService a = new AnalyticsService();
        //    decimal totalSpending = a.getTotalSpending(accID);

        //    // Print the total spending
        //    Console.WriteLine("For user #" + accID);
        //    Console.WriteLine("   Total spending: $" + totalSpending);
        //}

        //static void testCategoryBreakdown(long accID)
        //{
        //    AnalyticsService a = new AnalyticsService();
        //    Dictionary<string, Tuple<decimal, decimal>> breakdown = a.getCategoryBreakdown(accID);

        //    Console.WriteLine("For user #" + accID);
        //    Console.WriteLine("\nCategory Breakdown\n---------------\n");
        //    foreach(string category in breakdown.Keys)
        //    {
        //        Tuple<decimal, decimal> categoryVals = breakdown[category];
        //        Console.WriteLine("   " + category + ":");
        //        Console.WriteLine("      Category Total: $" + categoryVals.Item1);
        //        Console.WriteLine("      % of All Spending: " + categoryVals.Item2);
        //    }
        //}

        // ---------------------------------------------- Begin Subscription Testing ---------------------------------------------------------

        static void getSubscriptionNextID()
        {
            Console.WriteLine("\n-------------------- Test: Getting the next ID available, Write -------------------------");
            SubscriptionService subservice = new SubscriptionService();
            long SID = subservice.getNextAvailID();
            Console.WriteLine("Next available SID that can be assigned: " + SID + "\n");
        }
        
        static Subscription testAddSubscription()
        {
            Console.WriteLine("\n-------------------- Test: Creating A New Subscription, Write -------------------------");
            // Query DB for the next avail ID
            SubscriptionService subservice = new SubscriptionService();
            long SID = subservice.getNextAvailID();
            Console.WriteLine("Next available SID that can be assigned: " + SID + "\n");

            //Create a new Subscription 
            long tempRID = 2;
            long tempAccID = 2;

            Subscription sampleSub = new Subscription(SID, tempAccID, tempRID,new DateTime(2021, 02, 20, 0, 0, 0), (decimal)14.99, SubscriptionFrequency.Weekly);


            //Print summary of Subscription that was just created
            Console.WriteLine("Subscription Summary: \n-----------------------");
            Console.WriteLine(sampleSub);
            subservice.write(sampleSub);

            return sampleSub;
        }

        static Subscription testReinstantiateSubscription(long SID)
        {
            SubscriptionService subservice = new SubscriptionService();
            Subscription s = subservice.getUsingID(SID);
            return s;
        }

        static void testModifyRewriteSubscription(long SID)
        {
            SubscriptionService subservice = new SubscriptionService();
            Subscription s = subservice.getUsingID(SID);

            Console.WriteLine("This is the testModifyRewriteSubscrition test: \n");
            Console.WriteLine(" \nWe will change the purchase amount to 3x the amount. \n");
            //s.setAmount(s.getPurchaseAmount() * 3);
            s.setAmount(50);
            subservice.update(s);

        }

        static void testDeleteSubscription(long SID)
        {
            SubscriptionService subservice = new SubscriptionService();
            Console.WriteLine("We will delete ID: " + SID);
            subservice.delete(SID);
        }

        static void testDisplaySubscription(long SID)
        {
            SubscriptionService subservice = new SubscriptionService();
            Subscription s = subservice.getUsingID(SID);
            Console.WriteLine("\n Retrieving Subscription with SID {0} from the database....", SID);
            Console.WriteLine(subservice.getJSON(s));
            Console.WriteLine("Displaying summary of Subscription with SID {0}....", SID);
            Console.WriteLine(s);
        }

        // Displaying summary of Subscription
        static void testGetSubscriptionsFromAccount(long accID)
        {
            List<Subscription> subscriptions = new List<Subscription>();
            SubscriptionService subservice = new SubscriptionService();
            subscriptions = subservice.getSubscriptionsFromAccount(accID);
            Console.WriteLine("\nDisplaying all Subscriptions for user with account ID {0}...", accID);
            foreach (Subscription s in subscriptions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(s);
            }
        }

        //Displays summary of Subscriptions by Frequency
        static void testGetSubscriptionsByFrequency(long SID, string frequency)
        {
            List<Subscription> subscriptions = new List<Subscription>();
            SubscriptionService subservice = new SubscriptionService();
            subscriptions = subservice.getSubscriptionByRenewalFrequency(SID, frequency);
            Console.WriteLine("\nDisplaying all Subscriptions with ID of {0} with frequencies {1}...", SID, frequency);
            foreach (Subscription s in subscriptions)
            {
                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine(s);
            }

        }


        // --------------------------------------------------- End Testing ---------------------------------------------------



        static void Main(string[] args){
            // ---------------------------------------------- GCP Auth -------------------------------------------------------------------------
            authenticateGCP();


            //----------------------------------------------- Test: SG Create By End Date, Write------------------------------------------------
            //SavingsGoal sampleOne = testSGCreateByDate();


            // ---------------------------------------------- Test: SG Create By Contribution Amount, Write ------------------------------------
            //SavingsGoal sampleTwo = testSGCreateByContrAmt();


            // ---------------------------------------------- Test: Display String Serialization -----------------------------------------------
            //testSGShowSerialize(sampleOne);


            // ---------------------------------------------- Test: Display JSON Representation ------------------------------------------------
            //testSGShowJSON(sampleTwo);


            // ---------------------------------------------- Test: SG Read, Modify, Rewrite ------------------------------------------
            //testSGReadModifyRewrite(SGID: 3);

            // ---------------------------------------------- Test: Retrieve All SG From Account ------------------------------------------
            //testGetAllSGFromAcc(accID: 2);
            //testJSONGoalsList(accID: 2);

            //SavingsGoalService sgs = new SavingsGoalService();
            //long periods = sgs.getPeriodsSinceStart(sgs.getUsingID((long)24));
            //Console.WriteLine(periods);



            // ---------------------------------------------- Test: Creating User Account ------------------------------------------------------
            //testAccCreate();


            // ---------------------------------------------- Test: Transaction Create ---------------------------------------------------------
            //Transaction sampleTrans1 = testAddTransaction();
            //Transaction t = testReinstantiateTransaction(TID: 41);
            //TransactionService ts = new TransactionService();
            //Console.WriteLine(ts.getJSON(t));


            //Demo 3/25
            // FRAN-18 As a user, I want to be able to edit my existing transactions, so that I can correct or update any incorrect information.
            //testModifyRewriteTransaction(TID: 1);

            // FRAN-19 As a user, I want to be able to delete transactions, so that no longer wanted transactions are accurately represented in the log.
            //testDeleteTransaction(TID: 20);

            // FRAN-79 As a developer, I want to implement code to display a summary of the Transaction when printed into the console.
            //testDisplayTransaction(TID: 12);

            // FRAN-2 As a developer, I want to be able to display all the transactions associated with a given user.
            //testGetTransactionsFromAccount(accID: 1);

            //FRAN-76 As a developer, I want to write the queries to be able to sort a users transactions by any given TransactionCategory.
            //testGetTransactionsFromCategory(accID: 4, "HomeAndUtilities");


            // FRAN-7 As a developer, I want to be able to create a DB view of a user's transactions across any of the given time periods: { 1 day, 3 days, 1 week, 2 weeks, 1 month, 3 months, 6 months, 1 year, All time }
            // sorting past 3 months from today
            //testGetSortedTransactionsByTime(accID: 4, num: 3, choice: 2);

            // sorting past 1 week from today
            //testGetSortedTransactionsByTime(accID: 2, num: 1, choice: 1);


            // ---------------------------------------------- Test: Receipt Create ---------------------------------------------------------
            //Receipt r = testAddReceipt();
            //testSerializeReceipt(r);
            //testReceiptJSON(r);
            //Console.WriteLine(testReinstantiateReceipt(RID: 23));
            //testModifyRewriteReceipt(RID: 23);
            //testDeleteReceipt(RID: 11);



            // ---------------------------------------------- Test: Data Analytics ---------------------------------------------------------
            //testSpendingPerCategory(accID: 1);
            //testGetTotalSpending(accID: 1);
            //testCategoryBreakdown(accID: 1);
            //AnalyticsService ans = new AnalyticsService();
            //ans.getTopSpendingCategoryPercentages("frankfund");
            //ans.printTuples();
            //Console.WriteLine(ans.getJSON(ans.getSpendingPerMonthPastYear("frankfund")));
            //Console.WriteLine(ans.getJSON(ans.getAllTimeSpendingPerCategory("frankfund")));
            //Console.WriteLine(ans.getJSON(ans.getAllTimeCategoryBreakdown("frankfund")));



            // ---------------------------------------------- Test: Subscriptions ---------------------------------------------------------

            //Console.WriteLine("Let's add our first Subscription! \n");

            //getSubscriptionNextID();

            //Subscription firstSampleSub = testAddSubscription();

            //Subscription firstSampleTestR = testReinstantiateSubscription(9);

            //testDisplaySubscription(9);

            //testModifyRewriteSubscription(3);

            //Console.WriteLine("Let's display Subscription ID #3 to show the updates. \n");

            //testDisplaySubscription(3);

            //Console.WriteLine("\n");

            //testDeleteSubscription(7);

            //testGetSubscriptionsFromAccount(1);



            // ---------------------------------------------- Test: Receipt Upload/Download ---------------------------------------------------------
            //const string sampleImgPath = "../DataAccessLayer/tmp/upload/sample_receipt.jpeg";
            //testUploadReceipt(userName: "RachelPai", localPathToFile: sampleImgPath, fileName: "sample_receipt.jpeg");  // File overwrites if already exists
            //testDownloadReceipt(userName: "RachelPai", fileName: "sample_receipt.jpeg", dstPath: null);                 // Uses DataAccess/tmp/download by default (if null) for API





        }

    }
}