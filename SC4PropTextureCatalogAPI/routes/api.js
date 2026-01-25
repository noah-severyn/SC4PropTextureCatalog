import express from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { fileURLToPath } from 'url';
const sqlite = sqlite3.verbose();
const router = express.Router();

// Utility function to run queries
function ExecuteQuery(query, params = []) {
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
  const params = [];
  const wheres = [];
  let where = '';
  const fieldMap = { 
    assetid: 'ci.AssetId',
    file: 'ci.File',
    tgi: 'ci.TGI',
    category: 'cat.Name',
    name: 'ci.Name',
    package: 'pkg.PackageId',
    author: 'pkg.Author'
  };

  for (const [key, column] of Object.entries(fieldMap)) {
    console.log(request.query[key]);
    if (request.query[key]) {
      const term = CleanQueryText(request.query[key]);
      // console.log(`key:${key}, term:${term}`);
      if (term.length > 40) {
        return response.status(400).json({ error: 'Search term is too long' });
      } else if (term.length < 3) {
        return response.status(400).json({ error: 'Search term must be 3 characters minimum' });
      } else if (!Object.keys(fieldMap).includes(key)) {
        return response.status(400).json({ error: 'Invalid query term. Must be one of: ' + Object.keys(fieldMap).join(', ') });
      }
      wheres.push(`${column} LIKE ? ESCAPE '\\'`);
      params.push(`%${term}%`);

    }
  }

  // Handle if no fields are specified - search all columns
  if (wheres.length === 0) {
    const term = CleanQueryText(request.query.term || '');
    if (term.length > 40) {
      return response.status(400).json({ error: 'Search term is too long' });
    } else if (term.length < 3) {
      return response.status(400).json({ error: 'Search term must be 3 characters minimum' });
    }
    const like = `%${term}%`;
    where = `${fieldMap.assetid} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.file} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.tgi} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.category} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.name} LIKE ? ESCAPE '\\'
      OR ${fieldMap.package} LIKE ? ESCAPE '\\'
      OR ${fieldMap.author} LIKE ? ESCAPE '\\'`;
    params.push(like, like, like, like, like, like, like);
  } else {
    where = wheres.join('\n    AND ');
  }

  const query = `
    SELECT ci.ExchangeId, ci.AssetId, ci.File, ci.TGI, cat.Name AS Category, ci.Name, pkg.PackageId, pkg.Author
    FROM CatalogItems ci
    LEFT JOIN TGICategories cat ON ci.Category = cat.Category
    LEFT JOIN Packages pkg ON pkg.ExchangeId = ci.ExchangeId AND pkg.AssetId = ci.AssetId
    WHERE ${where}
    LIMIT 10000`;
  console.log(query);
  try {
    const results = await ExecuteQuery(query, params);
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
    const results = await ExecuteQuery(query, [like]);
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
    const results = await ExecuteQuery(query, [assetId]);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});


// GET /api/packages
router.get('/package', async (request, response) => {
  const searchText = CleanQueryText(request.query.term || '');

  if (searchText.length > 50) {
    return response.status(400).json({ error: 'search term too long' });
  } else if (searchText.length < 3 && searchText !== '') {
    return response.status(400).json({ error: 'search term must be 3 characters minimum' });
  }

  const query = (searchText !== '') ? `SELECT * FROM Packages WHERE PackageId LIKE ? ESCAPE '\\'` : `SELECT * FROM Packages`;
  const params = (searchText !== '') ? [`%${searchText}%`] : []
  try {
    const results = await ExecuteQuery(query, params);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});

// GET /api/dbstats
router.get('/dbstats', async (request, response) => {
  const query = `
    SELECT
      (SELECT COUNT(*) FROM Assets) AS Assets,
      (SELECT COUNT(*) FROM Files) AS Files,
      (SELECT COUNT(*) FROM Packages) AS Packages,
      (SELECT COUNT(*) FROM TGIs) AS TGIs,
	    (SELECT SUM(TextureCount) FROM Files) As Textures,
	    (SELECT SUM(PropCount) FROM Files) As Props,
	    (SELECT SUM(FloraCount) FROM Files) As Flora,
	    (SELECT SUM(BuildingCount) FROM Files) As Buildings`;
  const params = [];
  try {
    const results = await ExecuteQuery(query, params);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});

export default router;
