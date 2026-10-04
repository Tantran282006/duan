process.stdout.write('[BOOT] minimal-server starting\n');
const http = require('http');
const port = Number(process.env.PORT) || 3000;
process.stdout.write('[BOOT] PORT=' + port + '\n');
const server = http.createServer(function(req, res) {
  process.stdout.write('[REQ] ' + req.method + ' ' + req.url + '\n');
  res.writeHead(200, { 'Content-Type': 'application/json' });
  res.end(JSON.stringify({ ok: true, port: port, env: process.env.NODE_ENV }));
});
server.listen(port, '0.0.0.0', function() {
  process.stdout.write('[READY] Server listening on 0.0.0.0:' + port + '\n');
});
server.on('error', function(e) {
  process.stderr.write('[ERROR] ' + e.code + ': ' + e.message + '\n');
  process.exit(1);
});
process.on('uncaughtException', function(e) {
  process.stderr.write('[CRASH] ' + String(e) + '\n');
  process.exit(1);
});
