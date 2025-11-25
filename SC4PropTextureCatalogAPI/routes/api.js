import express from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { fileURLToPath } from 'url';
const sqlite = sqlite3.verbose();
const router = express.Router();

// Utility function to run queries
function runQuery(query, params = []) {
  return new Promise((resolve, reject) => {
    const __filename = fileURLToPath(import.meta.url);
    const __dirname = path.dirname(__filename);
    const dbPath = path.join(__dirname, '../data/Catalog.db');
    const db = new sqlite.Database(dbPath, sqlite3.OPEN_READONLY);
    db.all(query, params, (err, rows) => {
      db.close();
      if (err) reject(err);
      else resolve(rows);
    });
  });
}

function CleanQueryText(input) {
  return (input || '')
    .trim()
    .replace(/\\/g, '\\\\')
    .replace(/%/g, '\\%')
    .replace(/_/g, '\\_');
}

// GET /api/search?term=...
router.get('/search', async (request, response) => {
  const searchText = CleanQueryText(request.query.term);

  if (searchText.length > 50) {
    return response.status(400).json({ error: 'search term too long' });
  } else if (searchText.length < 3) {
    return response.status(400).json({ error: 'search term must be 3 characters minimum' });
  }

  const query = `
    SELECT ci.ExchangeId, ci.AssetId, ci.File, ci.TGI, cat.Name AS Category, ci.Name, pkg.PackageId, pkg.Author
    FROM CatalogItems ci
    LEFT JOIN OIN TGICategories cat ON ci.Category = cat.Category
    LEFT JOIN Packages pkg ON pkg.ExchangeId = ci.ExchangeId AND pkg.AssetId = ci.AssetId
    WHERE ci.AssetId LIKE ? ESCAPE '\\' OR
          ci.File LIKE ? ESCAPE '\\' OR
          ci.TGI LIKE ? ESCAPE '\\' OR
          ci.Name LIKE ? ESCAPE '\\'
    LIMIT 10000`;
  const like = `%${searchText}%`;
  try {
    const results = await runQuery(query, [like, like, like, like]);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});

// GET /api/iid?value=...
router.get('/iid', async (request, response) => {
  const iid = CleanQueryText(request.query.value);

  if (iid === '') {
    return response.status(400).json({ error: 'instance id must not be blank' });
  } else if (iid.length > 10) {
    return response.status(400).json({ error: 'search term too long' });
  }

  const query = `
    SELECT CatalogItems.AssetId, CatalogItems.File, substr(CatalogItems.TGI, -8) AS Instance, CatalogItems.TGI, TGICategories.Name AS Category, CatalogItems.Name
    FROM CatalogItems
    LEFT JOIN TGICategories ON CatalogItems.Category = TGICategories.Category
    WHERE substr(CatalogItems.TGI, -8) LIKE ?`;
  const like = `%${iid}%`;
  try {
    const results = await runQuery(query, [like]);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});

// GET /api/assetid?value=...
router.get('/assetid', async (request, response) => {
  const assetId = request.query.value || '';
  const query = `
    SELECT *
    FROM Assets
    WHERE AssetId = ?`;
  try {
    const results = await runQuery(query, [assetId]);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});

export default router;
