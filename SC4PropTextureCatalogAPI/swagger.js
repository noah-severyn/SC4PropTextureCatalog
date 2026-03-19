import swaggerAutogen from 'swagger-autogen';

const doc = {
  info: {
    title: 'SC4 Prop Texture Catalog API',
    description: 'An API for the <a href="https://community.simtropolis.com/forums/topic/758501-simcity-4-prop-and-texture-catalogue-by-stex-custodian">SimCity4 Prop & Texture Catalog</a>. Github: https://github.com/noah-severyn/SC4PropTextureCatalog',
    version: '1.1.0'
  }
};

const outputFile = './swagger-output.json';
const routes = ['./app.js'];

swaggerAutogen()(outputFile, routes, doc);

//use `npm run swagger` to rebuild the swagger page.
