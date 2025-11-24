import express from 'express';
import apiRoutes from './routes/api.js';
import cors from 'cors';

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors({Origin: 'http://127.0.0.1:3000' }));
app.use('/api', apiRoutes);

app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});
