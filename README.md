# MealPrepHelper 🥗

**MealPrepHelper** is a desktop application for meal planning, nutritional tracking, and pantry management. It is built on the **.NET** platform using the **Avalonia UI** framework and the **MVVM (ReactiveUI)** architecture.

## Key Features

### 👤 Profile & Goals
* **Registration & Login:** User profile creation with automatic nutrition calculation.
* **Smart Calculator:** Automatic calculation of daily calorie and macro goals (Proteins, Carbs, Fats, Fiber) based on the selected goal (**Weight Loss / Maintenance / Muscle Gain**).
* **Profile Editing:** Ability to update weight, age, or activity level at any time with automatic goal recalculation.

### 📅 Planning Calendar
* **Monthly Overview:** Visual calendar indicating days with scheduled meals.
* **Daily Detail:** Popup window for adding meals (Breakfast, Lunch, Dinner...).
* **Recipe Selection:** Easy insertion of recipes from the database.

### 📊 Daily Dashboard
* **Macro Tracking:** Pie charts displaying current intake vs. daily goals.
* **Color Indication:** Charts turn red if the daily limit is exceeded.
* **Weekly Bar:** Quick navigation between days.
* **Ingredient Check:** Recipe details show whether you have the necessary ingredients in your pantry (✅ Owned / ❌ Missing).

### 🏠 Pantry
* **Inventory Management:** Overview of items currently in stock (including amounts and units).
* **Add/Remove:** Simple form for restocking and buttons for consuming stock.

### 🛒 Shopping List
* **Smart Replenishment:** The app calculates exactly how much of an ingredient is missing for a recipe.
* **Finish Shopping:** The **"Finish Shopping"** button automatically moves purchased items to the Pantry and clears the list.
* **Manual Adjustments:** Ability to manually add items or adjust quantities directly in the list.

---

## 🛠 Tech Stack

* **Language:** C# (.NET 9)
* **UI Framework:** Avalonia UI (Cross-platform XAML)
* **Architecture:** MVVM (Model-View-ViewModel)
* **Reactivity:** ReactiveUI
* **Database:** SQLite (Entity Framework Core)
* **Icons/Graphics:** Standard XAML shapes and Unicode emojis.

---

## 📦 Installation & Setup

### Requirements
* **The .NET SDK** (version 9.0) installed.

### Steps
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/TomasIsNotHere/MealPrepHelper.git
    cd MealPrepHelper
    ```

2.  **Restore packages:**
    ```bash
    dotnet restore
    ```

3.  **Run the application:**
    ```bash
    dotnet run
    ```

---

## 🗂 Project Structure

The project follows a strict MVVM structure:

* 📂 **Models:** Database entities (`User`, `Recipe`, `Ingredient`, `PantryItem`...).
* 📂 **ViewModels:** Application logic, ReactiveCommands (`OverviewViewModel`, `CalendarViewModel`...).
* 📂 **Views:** XAML files defining the UI (`UserControl`, `Window`).
* 📂 **Data:** Database context (`AppDbContext`) and initializer (`DbInitializer`).
* 📂 **Services:** Helper classes (`NutritionCalculator`, `PasswordHelper`).

---

## 🔑 Demo Credentials

The application creates a demo user upon the first launch:

* **Login:** `Admin`
* **Password:** `admin123`

---

## 📝 Roadmap (To-Do)

* [ ] Recipe Editor (allow users to create custom recipes).
* [ ] Export Shopping List to PDF/Text.
* [ ] Weight history and progress charts.
