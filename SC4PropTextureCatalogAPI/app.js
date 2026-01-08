import express from 'express';
import apiRoutes from './routes/api.js';
import cors from 'cors';

const app = express();
const PORT = process.env.PORT || 4000;

const allowedOrigins = [
  'http://127.0.0.1:4000',
  'http://localhost:4000',
  'https://your-frontend-domain.onrailway.app'
];
app.use(cors({
  origin: allowedOrigins,
  methods: ['GET', 'POST', 'PUT', 'DELETE'],
  allowedHeaders: ['Content-Type']
}));

app.use('/api', apiRoutes);

app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});
