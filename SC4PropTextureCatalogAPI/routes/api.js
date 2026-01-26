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
  // #swagger.summary = 'Search for TGIs via multiple fields'
  // #swagger.description = 'Primary search utility. Search across packages, files, TGIs, and exemplar names. Supports field-specific searches or a general search across all fields. Returns up to 2000 results.'
  /* #swagger.parameters['term'] = {
      description: 'General search term applied to all searchable fields (used when no specific field is provided)'
  } */
  /* #swagger.parameters['package'] = {
      description: 'sc4pac package identifier in the format group:name'
  } */
  /* #swagger.parameters['author'] = {
      description: 'Package author'
  } */
  /* #swagger.parameters['subfolder'] = {
      description: 'Package subfolder'
  } */
  /* #swagger.parameters['file'] = {
      description: 'File name within package'
  } */
  /* #swagger.parameters['tgi'] = {
      description: 'TGI'
  } */
  /* #swagger.parameters['category'] = {
      description: 'TGI category'
  } */
  /* #swagger.parameters['name'] = {
      description: 'Exemplar or item name'
  } */
  /* #swagger.responses[400] = {
      description: 'Bad request - search term too long or too short, or invalid field specified'
  } */
  const params = [];
  const wheres = [];
  let where = '';
  const fieldMap = { 
    package: 'Packages.Name',
    author: 'Packages.Author',
    subfolder: 'Packages.Subfolder',
    file: 'Files.Name',
    tgi: 'TGIs.TGI',
    category: 'TGICategories.Name',
    name: 'TGIs.Name',
  };

  for (const [key, column] of Object.entries(fieldMap)) {  
    if (request.query[key]) {
      const term = CleanQueryText(request.query[key]);
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
    where = `${fieldMap.package} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.author} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.file} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.tgi} LIKE ? ESCAPE '\\' 
      OR ${fieldMap.category} LIKE ? ESCAPE '\\'
      OR ${fieldMap.name} LIKE ? ESCAPE '\\'`;
    params.push(like, like, like, like, like, like);
  } else {
    where = wheres.join('\n    AND ');
  }

  const query = `
    SELECT Packages.Name Package, Packages.Subfolder, Packages.Websites, Packages.Author, Files.Name FileName, TGIs.TGI, TGICategories.Name Category, TGIs.Name ExemplarName
    FROM Packages
    LEFT JOIN PackageFiles ON PackageFiles.PackageId = Packages.Id
    LEFT JOIN Files ON Files.Id = PackageFiles.FileId
    LEFT JOIN TGIs ON TGIs.FileId = Files.Id
    LEFT JOIN TGICategories on TGICategories.Id = TGIs.Category
    WHERE ${where}
    LIMIT 2000`;
  try {
    const results = await ExecuteQuery(query, params);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});


// GET /api/iid?value=...
router.get('/iid', async (request, response) => {
  // #swagger.summary = 'Search for TGIs via IID'
  // #swagger.description = 'Search for resources by Instance ID (IID). Searches the last 8 characters of TGI identifiers to find matching instance IDs.'
  /* #swagger.parameters['value'] = {
      description: 'IID to search for. Values with and without a preceding 0x are supported.'
  } */
  /* #swagger.responses[400] = {
      description: 'Bad request - IID is blank or too long'
  } */
  const iid = CleanQueryText(request.query.value);

  if (iid === '') {
    return response.status(400).json({ error: 'instance id must not be blank' });
  } else if (iid.length > 10) {
    return response.status(400).json({ error: 'search term too long' });
  }

  const query = `
    SELECT Packages.Name Package, TGIs.TGI, TGICategories.Name Category, TGIs.Name ExemplarName
    FROM Packages
    LEFT JOIN PackageFiles ON PackageFiles.PackageId = Packages.Id
    LEFT JOIN Files ON Files.Id = PackageFiles.FileId
    LEFT JOIN TGIs ON TGIs.FileId = Files.Id
    LEFT JOIN TGICategories on TGICategories.Id = TGIs.Category
    WHERE substr(TGIs.TGI, -8) LIKE ?`;
  const like = `%${iid}%`;
  try {
    const results = await ExecuteQuery(query, [like]);
    response.json(results);
  } catch (err) {
    response.status(500).json({ error: 'Database error', details: err.message });
  }
});


// // GET /api/assetid?value=...
// router.get('/assetid', async (request, response) => {
//   const assetId = request.query.value || '';
//   const query = `
//     SELECT *
//     FROM Assets
//     WHERE AssetId = ?`;
//   try {
//     const results = await ExecuteQuery(query, [assetId]);
//     response.json(results);
//   } catch (err) {
//     response.status(500).json({ error: 'Database error', details: err.message });
//   }
// });


// GET /api/packages?term=...
router.get('/package', async (request, response) => {
  // #swagger.summary = 'Search packages'
  // #swagger.description = 'Search for packages and retrieve aggregate statistics. If no search term is provided, all packages are returned with their statistics.'
  /* #swagger.parameters['term'] = {
      description: 'sc4pac package id in the group:name format. If omitted, all packages are returned.'
  } */
  /* #swagger.responses[400] = {
      description: 'Bad request - Search term too long or too short'
  } */
  const searchText = CleanQueryText(request.query.term || '');

  if (searchText.length > 50) {
    return response.status(400).json({ error: 'search term too long' });
  } else if (searchText.length < 3 && searchText !== '') {
    return response.status(400).json({ error: 'search term must be 3 characters minimum' });
  }

  const query = `
    SELECT Packages.Name Package, Packages.Version, Packages.Subfolder, Packages.Websites, Packages.Author, SUM(Files.TextureCount) Textures, SUM(Files.PropCount) Props, SUM(Files.FloraCount) Flora, SUM(Files.BuildingCount) Buildings
    FROM Packages
    LEFT JOIN PackageFiles ON PackageFiles.PackageId = Packages.Id
    LEFT JOIN Files ON Files.Id = PackageFiles.FileId
    ${searchText !== '' ? 'WHERE Packages.Name LIKE ?' : ''}
    GROUP BY Packages.Name
    `;
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
  // #swagger.summary = 'Get database statistics'
  // #swagger.description = 'Retrieve database statistics about the total count of items in each table.'
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
