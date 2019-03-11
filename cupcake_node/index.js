const express = require('express')
const app = express()
const port = 3000
const { Pool } = require('pg')
const QueryStream = require('pg-query-stream')
const JSONStream = require('jsonstream')
const es = require('event-stream');


app.listen(port, () => console.log(`Example app listening on port ${port}!`))

const pool = new Pool({
    user: process.env.CUPCAKE_DBUSER || 'cupcake_svc',
    host: process.env.CUPCAKE_DBSERVER || 'localhost',
    database: process.env.CUPCAKE_DBNAME || 'cupcake',
    password: process.env.CUPCAKE_DBPASSWORD || 'cupcakesvc123',
    port: process.env.CUPCAKE_DBPORT || 5432,
});



app.get('/get', (req, res) => {
    //pipe 1,000,000 rows to stdout without blowing up your memory usage
    pool.connect((err, client, done) => {
        if (err) throw err;

        //var start = now();
        const query = new QueryStream('SELECT * from test_data')
        const stream = client.query(query)
        //release the client when the stream is finished        
        stream.on('end', () => {
            res.write("]");
            done();
        });
        //stream.pipe(JSONStream.stringify()).pipe(res);
        res.write("[");
        stream.pipe(es.map( (data,cb) => cb(null,'"' + data.data + '",')  )).pipe(res);
    })
   
})