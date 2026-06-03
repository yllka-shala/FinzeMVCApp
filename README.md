# 🚀 FinanceApp (Finze)
A fullstack MVC based web application built with .NET 8 and C# to help users track and manage their personal expenses.

## ✨ Key Features
Finze web application is a system that allows users to:

  * Manage their accounts (registering, verifying account, accessing their profile, updating data and resetting password).
  * Setting their Budgets.
  * Tracking their financial transactions.
  * Registering categories for their expenses.
  * Adding bill reminders for recurring payments.
  * Setting financial goals.
  * Adding recurring bills based on frequency of recurrence.
  * Managing their financial accounts, this module represents users personal account, business account, and similar accounts.
  * Receiving notifications about their account activity or updates.
  * Checking different charts related to their data.
  * Exporting their data in two formats: excel and pdf.
  * Searching data, adding, updating and deleting.
  * Provides a Helping page for users.

## 🛠 Tech Stack

 * **Language:** C#
 * **Framework:** .NET 8 / ASP.NET Core MVC
 * **Database:** SQL Server 
 * **UI:** CSS / Bootstrap / JavaScript
 * **ORM:** Entity Framework Core / LINQ
 * **Security:** Encrypted user authentication, role based authorization (Admin_Role and Customer_Role).
 * **Data Visualization:** Interactive charts using Chart.js.
 * **Export to files:** Export data to Excel (ClosedXML) or Pdf (QuestPdf).
 ***Logging:** Logging to files (Serilog).

## 🚀 Getting Started
To run this project locally, follow these steps:

  1. **Clone the repo:**
    git clone https://github.com/yllka-shala/FinzeMVCApp.git
  2. **Open the solution:**
    Open `FinanceApp.sln` in Visual Studio.
  3. **Open the AppSettings.json file:**
    Add your own "Your_Server", "Your_Email" and "Your_Password".
  4. **Migrate database:**
    In Package Manager Console add command Update-Database to create the database in MSSQL.
  5. **Run:**
    Press `F5` to build and launch the app.
