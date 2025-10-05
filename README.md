# 🎧 PodWave Player

**End of Apprenticeship Project**  
A desktop application to **add, play, and manage podcasts** directly on your computer.  
Built with **C#**, **WPF**, and **MySQL**.

---
⚠️ Disclaimer

This repository reflects the state of the PodWave Player at the time of my final apprenticeship project submission (Summer 2025).
It was created to document and showcase the completed project as it was officially submitted for examination.

Further developments, refactoring, and experimental features that were added after the submission period are not included in this version to preserve the integrity of the original project state.
---

## 🚀 Overview

**PodWave Player** is a lightweight podcast player for the desktop.  
It allows users to:
- Import podcasts via **RSS-Feed**
- Browse and play episodes
- Automatically save and restore playback progress
- Store podcast and episode data in a local SQL database

---

## 🧩 Features

✅ Add new podcasts via RSS feed  
✅ Display podcast details and cover images  
✅ Play, pause, and skip between episodes  
✅ Save playback progress in the database  
✅ Resume from the last played position  
✅ Adjustable volume and progress slider  
✅ Persistent storage via MySQL  
✅ Simple and clean WPF UI  

---

## 💾 Database Schema (MySQL)

CREATE DATABASE IF NOT EXISTS podwave_db 
    DEFAULT CHARACTER SET utf8mb4 
    COLLATE utf8mb4_general_ci;

USE podwave_db;

DROP TABLE IF EXISTS podcast;
CREATE TABLE podcast (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    DescriptionP TEXT,
    FeedUrl VARCHAR(500),
    ImageUrl VARCHAR(500)
);

DROP TABLE IF EXISTS episode;
CREATE TABLE episode (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PodcastId INT NOT NULL,
    Title VARCHAR(255) NOT NULL,
    DescriptionE TEXT,
    AudioUrl VARCHAR(500),
    DurationInSeconds INT DEFAULT 0,
    FOREIGN KEY (PodcastId) REFERENCES podcast(Id) ON DELETE CASCADE
);

DROP TABLE IF EXISTS playbackprogress;
CREATE TABLE playbackprogress (
    EpisodeId INT PRIMARY KEY,
    PositionInSeconds INT DEFAULT 0,
    FOREIGN KEY (EpisodeId) REFERENCES episode(Id) ON DELETE CASCADE
);
💡 Run this SQL script in your MySQL environment (e.g., phpMyAdmin or MySQL Workbench) before launching the application.

🛠️ Technologies Used:

  C# / WPF (Windows Presentation Foundation) – for the user interface
  
  MySqlConnector (NuGet) – for database communication
  
  System.ServiceModel.Syndication – for parsing RSS feeds
  
  DispatcherTimer – to update playback progress in real time
  
  XAML – for UI design


⚙️ Installation

  Clone this repository: git clone https://github.com/<yourusername>/PodWave-Player.git
  Create the MySQL database using the SQL script above.
  Open the project in Visual Studio 2022.
  Check that the connection string in DatabaseHelper.cs matches your setup: private const string ConnectionString = "Server=localhost;Database=podwave_db;Uid=root;Pwd=;";
  Run the application and start adding RSS feeds 🎧

  ---------------------------------------------
  💬 Author

          Lea Bürkle
          📍 Germany
          🎓 Software Development Apprentice (2025)
          
