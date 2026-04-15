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

## 📸 Screenshots

<img width="1917" height="903" alt="AdminDashboard" src="https://github.com/user-attachments/assets/ec26447e-51d6-4df8-8909-7d2759acd507" />
<img width="1887" height="896" alt="Transactions" src="https://github.com/user-attachments/assets/5e0b0f4f-cedf-443b-9c6a-1641e42e6791" />
<img width="1893" height="903" alt="UserReports" src="https://github.com/user-attachments/assets/37385277-db4a-46dc-9fea-7f112de77adf" />
<img width="1912" height="907" alt="Login" src="https://github.com/user-attachments/assets/9f4821e7-3ae9-4aff-ac7d-048785f4db31" />
<img width="1905" height="901" alt="UserProfile" src="https://github.com/user-attachments/assets/a7d5fe96-60ec-4b4b-8a45-dcba4ec0ab38" />
<img width="1902" height="902" alt="Register" src="https://github.com/user-attachments/assets/fde72672-74c3-4e3b-a204-aaccac0f2631" />

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
