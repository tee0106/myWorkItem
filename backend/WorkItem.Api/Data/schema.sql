PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS work_item (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  title TEXT NOT NULL,
  description TEXT NOT NULL DEFAULT '',
  created_at TEXT NOT NULL,
  updated_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS confirmation (
  display_name TEXT NOT NULL,
  work_item_id INTEGER NOT NULL,
  PRIMARY KEY (display_name, work_item_id),
  FOREIGN KEY (work_item_id) REFERENCES work_item(id) ON DELETE CASCADE
);
