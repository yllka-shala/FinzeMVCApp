# 🚀 FinanceApp (Finze)
A fullstack MVC based web application built with .NET 8 and C# to help users track and manage their personal expenses.

## ✨ Key Features
Finze web application is a system that allows users to:

  1. Manage their accounts (registering, verifying account, accessing their profile, updating data and resetting password).
  2. Setting their Budgets.
  3. Tracking their financial transactions.
  4. Registering categories for their expenses.
  5. Adding bill reminders for recurring payments.
  6. Setting financial goals.
  7. Adding recurring bills based on frequency of recurrence.
  8. Managing their financial accounts, this module represents users personal account, business account, and similar accounts.
  9. Receiving notifications about their account activity or updates.
  10. Checking different charts related to their data.
  11. Exporting their data in two formats: excel and pdf.
  12. Searching data, adding, updating and deleting.
  13. Provides a Helping page for users.

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
  3. **Migrate database:**
    In Package Manager Console add command Update-Database to create the database in MSSQL.
  4. **Run:**
    Press `F5` to build and launch the app.
