const express = require('express');
const fs = require('fs');
const sqlite3 = require('sqlite3').verbose();
const router = express.Router();

// SQLite endpoint
// router.get('/sqlite', (req, res) => {
//   const db = new sqlite3.Database('./data/Catalog.db', sqlite3.OPEN_READONLY);
//   db.all('SELECT * FROM CatalogItems', [], (err, rows) => {
//     if (err) return res.status(500).json({ error: 'Failed to query database' });
//     res.json(rows);
//   });
//   db.close();
// });

// Utility function to run queries
function runQuery(query, params = []) {
  return new Promise((resolve, reject) => {
    const db = new sqlite3.Database('./data/Catalog.db', sqlite3.OPEN_READONLY);
    db.all(query, params, (err, rows) => {
      db.close();
      if (err) reject(err);
      else resolve(rows);
    });
  });
}

// GET /api/search?term=...
router.get('/search', async (req, res) => {
  const search = req.query.term || '';
  const query = `
    SELECT CatalogItems.AssetId, CatalogItems.File, CatalogItems.TGI, TGICategories.Name AS Category, CatalogItems.Name
    FROM CatalogItems
    LEFT JOIN TGICategories ON CatalogItems.Category = TGICategories.Category
    WHERE CatalogItems.AssetId LIKE ? OR
          CatalogItems.File LIKE ? OR
          CatalogItems.TGI LIKE ? OR
          CatalogItems.Name LIKE ?
  `;
  const like = `%${search}%`;
  try {
    const results = await runQuery(query, [like, like, like, like]);
    res.json(results);
  } catch (err) {
    res.status(500).json({ error: 'Database error', details: err.message });
  }
});

// GET /api/instance?value=...
router.get('/instance', async (req, res) => {
  const instance = req.query.value || '';
  const query = `
    SELECT CatalogItems.AssetId, CatalogItems.File, substr(CatalogItems.TGI, -8) AS Instance, CatalogItems.TGI, TGICategories.Name AS Category, CatalogItems.Name
    FROM CatalogItems
    LEFT JOIN TGICategories ON CatalogItems.Category = TGICategories.Category
    WHERE substr(CatalogItems.TGI, -8) LIKE ?
  `;
  const like = `%${instance}%`;
  try {
    const results = await runQuery(query, [like]);
    res.json(results);
  } catch (err) {
    res.status(500).json({ error: 'Database error', details: err.message });
  }
});

module.exports = router;
