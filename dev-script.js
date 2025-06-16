#!/usr/bin/env node

const { spawn } = require("child_process");
const fs = require("fs");
const path = require("path");

const COMMANDS = {
  setup: "Configura o projeto",
  build: "Build de produção",
  test: "Executa todos os testes",
  dev: "Inicia ambiente de desenvolvimento",
  docker: "Inicia containers",
  clean: "Remove artefatos",
  quality: "Executa análise com Sonar",
};

function showHelp() {
  console.log("Uso: npm run dev-script <comando>");
  Object.entries(COMMANDS).forEach(([cmd, desc]) => {
    console.log(`  ${cmd.padEnd(10)} - ${desc}`);
  });
}

function execCmd(command, cwd = process.cwd()) {
  return new Promise((resolve, reject) => {
    const child = spawn(command, {
      shell: true,
      stdio: "inherit",
      cwd,
    });
    child.on("close", (code) => {
      if (code === 0) resolve();
      else reject(new Error(`Falha ${code}`));
    });
  });
}

function checkDependencies() {
  const deps = ["docker", "dotnet", "node"];
  for (const dep of deps) {
    try {
      require("child_process").execSync(`${dep} --version`, {
        stdio: "ignore",
      });
    } catch {
      console.error(`${dep} não encontrado`);
      process.exit(1);
    }
  }
}

async function setup() {
  checkDependencies();

  await execCmd("dotnet restore", "api");
  await execCmd("dotnet build", "api");

  if (fs.existsSync("web/node_modules"))
    fs.rmSync("web/node_modules", { recursive: true, force: true });
  if (fs.existsSync("web/package-lock.json"))
    fs.unlinkSync("web/package-lock.json");

  await execCmd("npm install", "web");
  await execCmd("docker compose up db rabbitmq -d");
}

async function build() {
  await execCmd("dotnet build --configuration Release", "api");
  await execCmd("npm run build", "web");
}

async function test() {
  await execCmd("dotnet test --configuration Release", "api");
  await execCmd("npm test", "web");
  await execCmd("npm run lint", "web");
}

async function dev() {
  await execCmd("docker compose up db rabbitmq -d");

  console.log("Execute:");
  console.log("  cd api && dotnet run --project Ecommerce.Api");
  console.log("  cd web && npm run dev");
}

async function docker() {
  await execCmd("docker compose up --build");
}

async function clean() {
  const paths = [
    "api/bin",
    "api/obj",
    "web/dist",
    "web/node_modules",
    "web/coverage",
  ];

  for (const p of paths) {
    if (fs.existsSync(p)) {
      fs.rmSync(p, { recursive: true, force: true });
    }
  }
}

async function quality() {
  try {
    require("child_process").execSync(
      "curl -s http://localhost:9000/api/system/status",
      { stdio: "ignore" }
    );
  } catch {
    await execCmd("docker compose up sonarqube -d");

    let retries = 30;
    while (retries > 0) {
      try {
        require("child_process").execSync(
          'curl -s http://localhost:9000/api/system/status | grep -q "UP"',
          { stdio: "ignore" }
        );
        break;
      } catch {
        await new Promise((r) => setTimeout(r, 10000));
        retries--;
      }
    }
  }

  await execCmd(
    'dotnet sonarscanner begin /k:"ecommerce-api" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin" /d:sonar.password="admin"',
    "api"
  );
  await execCmd("dotnet build", "api");
  await execCmd(
    'dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"',
    "api"
  );
  await execCmd("npm run sonar", "web");
}

const command = process.argv[2];

if (!command || !COMMANDS[command]) {
  showHelp();
  process.exit(1);
}

(async () => {
  try {
    switch (command) {
      case "setup":
        await setup();
        break;
      case "build":
        await build();
        break;
      case "test":
        await test();
        break;
      case "dev":
        await dev();
        break;
      case "docker":
        await docker();
        break;
      case "clean":
        await clean();
        break;
      case "quality":
        await quality();
        break;
    }
  } catch (error) {
    console.error("Erro:", error.message);
    process.exit(1);
  }
})();
