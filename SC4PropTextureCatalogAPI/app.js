import express from 'express';
import apiRoutes from './routes/api.js';
import cors from 'cors';
import swaggerUi from 'swagger-ui-express';
import swaggerFile from './swagger-output.json' with { type: 'json' };

const app = express();
const PORT = process.env.PORT || 4000;

const allowedOrigins = [
  'http://127.0.0.1:3000', //Website live previewer in VS Code
  'http://localhost:3000',
  'http://127.0.0.1:4000', //Local Swagger UI docs page 
  'http://localhost:4000',
  'https://sc4proptexturecatalog-production.up.railway.app',
  'https://noah-severyn.github.io',
  'https://sc4proptexturecatalog.net',
];
app.use(cors({
  origin: allowedOrigins,
  methods: ['GET'],
  allowedHeaders: ['Content-Type']
}));

// Dynamically serve Swagger with the correct host so it works with localhost and production
app.use('/docs', swaggerUi.serve, (req, res, next) => {
  const protocol = req.protocol;
  const host = req.get('host');
  const swaggerDoc = { ...swaggerFile };
  delete swaggerDoc.host;
  delete swaggerDoc.schemes;
  swaggerUi.setup(swaggerDoc, {
    swaggerOptions: {
      persistAuthorization: true,
      tryItOutEnabled: true,
      url: `${protocol}://${host}/docs/swagger.json`
    }
  })(req, res, next);
});

app.use('/api', apiRoutes);

app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}. See docs at /docs.`);
});

// Use `npx nodemon app.js` to test the api locally