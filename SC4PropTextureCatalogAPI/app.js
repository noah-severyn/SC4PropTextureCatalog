const express = require('express');
const apiRoutes = require('./routes/api');

const app = express();
const PORT = process.env.PORT || 3000;

app.use('/api', apiRoutes);

app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});

app.get('/debug/list', (req, res) => {
  const p = path.resolve(__dirname, '..');
  const out = {};
  function walk(dir) {
    try {
      out[dir] = fs.readdirSync(dir);
      out[dir].forEach(f => {
        const fp = path.join(dir, f);
        if (fs.statSync(fp).isDirectory()) walk(fp);
      });
    } catch(e) { out[dir] = 'err: ' + e.message; }
  }
  walk(p);
  res.json(out);
});