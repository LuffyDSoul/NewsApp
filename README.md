# 📰 NewsApp - Sistema de Noticias y Alertas

## 🎓 Información Académica

**Universidad**: Universidad Tecnológica Nacional - Facultad Regional Concepción del Uruguay (UTN-FRCU)  
**Materia**: Desarrollo de Software  
**Profesores**: 
- Ing. Enzo Tanga
- Ing. Esteban Tripodi

**Equipo de Desarrollo**:
- Pablo Valentín Leal
- Omar Sebastián Moreyra
- Ignacio Gabriel Martinez
- Valentín Schultheis
- Leandro Guiffrey

**Año**: 2026

---

## 📋 Descripción del Proyecto

NewsApp es una aplicación web completa para la gestión y consulta de noticias en tiempo real, con sistema de alertas personalizadas. Los usuarios pueden buscar noticias, configurar alertas basadas en palabras clave, y recibir notificaciones cuando se publiquen noticias relacionadas con sus intereses.

### Funcionalidades Principales

- 🔍 **Búsqueda de Noticias**: Búsqueda en tiempo real utilizando NewsAPI.org
- 📌 **Sistema de Alertas**: Crear alertas personalizadas basadas en palabras clave
- 🔔 **Notificaciones**: Recibir notificaciones cuando se encuentren noticias relevantes
- 👤 **Perfil de Usuario**: Gestión de información personal y preferencias
- 📚 **Listas de Lectura**: Guardar y organizar noticias de interés
- 🔐 **Autenticación**: Sistema completo de registro, login y recuperación de contraseña
- 📧 **Notificaciones por Email**: Alertas enviadas por correo electrónico
- 🎨 **Temas Personalizables**: Interfaz adaptable con múltiples temas

---

## 🛠️ Tecnologías Utilizadas

### Backend
- **.NET 8.0**: Framework principal del backend
- **ABP Framework 8.3.3**: Framework de aplicación modular con DDD
- **Entity Framework Core 8.0**: ORM para acceso a datos
- **SQL Server 2019+**: Base de datos relacional
- **OpenIddict**: Autenticación OAuth 2.0 / OpenID Connect
- **NewsAPI.org**: API externa para obtención de noticias
- **System.Net.Mail**: Servicio SMTP para envío de emails

### Frontend
- **Angular 17**: Framework SPA
- **TypeScript 5.2**: Lenguaje de programación
- **Bootstrap 5**: Framework CSS
- **RxJS**: Programación reactiva
- **ABP NG.Core**: Librerías Angular de ABP

### Arquitectura
- **Domain-Driven Design (DDD)**: Patrón de diseño de software
- **Repository Pattern**: Patrón de acceso a datos
- **Dependency Injection**: Inversión de control
- **CQRS**: Separación de comandos y consultas

---

## ✅ Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

1. **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js 18.x o superior** - [Descargar](https://nodejs.org/)
3. **SQL Server 2019+** o **SQL Server Express** - [Descargar](https://www.microsoft.com/sql-server/sql-server-downloads)
4. **Visual Studio 2022** o **VS Code** (opcional pero recomendado)
5. **SQL Server Management Studio** (SSMS) - Para gestión de base de datos

---

## 🚀 Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone https://github.com/tu-usuario/NewsApp.git
cd NewsApp
```

### 2. Configurar Base de Datos

1. Abre SQL Server Management Studio
2. Crea una nueva base de datos llamada `NewsApp`
3. Actualiza la cadena de conexión en `src/NewsApp.DbMigrator/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=NewsApp;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Ejecutar Migraciones

```bash
cd src/NewsApp.DbMigrator
dotnet run
```

Este comando creará todas las tablas y datos iniciales en la base de datos.

### 4. Configurar NewsAPI Key

1. Regístrate en [NewsAPI.org](https://newsapi.org/) y obtén una API key gratuita
2. Actualiza la configuración en `src/NewsApp.HttpApi.Host/appsettings.json`:

```json
{
  "NewsApi": {
    "ApiKey": "TU_API_KEY_AQUI",
    "BaseUrl": "https://newsapi.org/v2/"
  }
}
```

### 5. Configurar SMTP (Opcional - para emails)

En `src/NewsApp.HttpApi.Host/appsettings.json`:

```json
{
  "Settings": {
    "Abp.Mailing.Smtp.Host": "smtp.gmail.com",
    "Abp.Mailing.Smtp.Port": "587",
    "Abp.Mailing.Smtp.UserName": "tu-email@gmail.com",
    "Abp.Mailing.Smtp.Password": "tu-contraseña",
    "Abp.Mailing.Smtp.EnableSsl": "true",
    "Abp.Mailing.DefaultFromAddress": "tu-email@gmail.com"
  }
}
```

### 6. Instalar Dependencias del Frontend

```bash
cd NewsApp.Angular
npm install
```

### 7. Configurar API URL del Frontend

En `NewsApp.Angular/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  application: {
    baseUrl: 'http://localhost:4200',
    name: 'NewsApp',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44341/',
    redirectUri: 'http://localhost:4200',
    clientId: 'NewsApp_App',
    responseType: 'code',
    scope: 'offline_access NewsApp',
  },
  apis: {
    default: {
      url: 'https://localhost:44341',
      rootNamespace: 'NewsApp',
    },
  },
};
```

---

## ▶️ Ejecutar la Aplicación

### Opción 1: Scripts Automáticos (Recomendado)

**Windows**:
```bash
start-dev.bat
```

**Linux/Mac**:
```bash
chmod +x start-dev.sh
./start-dev.sh
```

### Opción 2: Ejecución Manual

**Terminal 1 - Backend**:
```bash
cd src/NewsApp.HttpApi.Host
dotnet run
```

**Terminal 2 - Frontend**:
```bash
cd NewsApp.Angular
npm start
```

### URLs de Acceso

- **Frontend**: http://localhost:4200
- **Backend API**: https://localhost:44341
- **Swagger UI**: https://localhost:44341/swagger

### Credenciales Iniciales

- **Usuario**: admin
- **Contraseña**: 1q2w3E*

---

## 📐 Arquitectura del Proyecto

El proyecto sigue los principios de **Domain-Driven Design (DDD)** y está organizado en capas:

### Estructura de Capas

```
NewsApp/
├── src/
│   ├── NewsApp.Domain/              # Capa de Dominio
│   │   ├── Entities/                # Entidades del dominio
│   │   ├── Repositories/            # Interfaces de repositorios
│   │   └── Services/                # Servicios de dominio
│   │
│   ├── NewsApp.Application/         # Capa de Aplicación
│   │   ├── Services/                # Application Services
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Mappers/                 # AutoMapper profiles
│   │
│   ├── NewsApp.EntityFrameworkCore/ # Capa de Infraestructura
│   │   ├── EntityFrameworkCore/     # DbContext
│   │   ├── Repositories/            # Implementaciones de repositorios
│   │   └── Migrations/              # Migraciones de EF Core
│   │
│   ├── NewsApp.HttpApi/             # Capa de API
│   │   └── Controllers/             # Controladores API
│   │
│   └── NewsApp.HttpApi.Host/        # Punto de entrada
│       └── appsettings.json         # Configuración
│
└── NewsApp.Angular/                 # Frontend Angular
    └── src/
        └── app/
            ├── features/            # Módulos de funcionalidades
            ├── shared/              # Componentes compartidos
            └── core/                # Servicios core
```

### Módulos Principales

1. **News**: Gestión y búsqueda de noticias
2. **NewsAlerts**: Sistema de alertas personalizadas
3. **Notifications**: Sistema de notificaciones
4. **UserProfile**: Gestión de perfiles de usuario
5. **ReadingLists**: Listas de lectura personalizadas
6. **Monitoring**: Monitoreo de alertas y procesamiento

---

## 🔌 Endpoints Principales de la API

### Noticias
- `GET /api/news/test-connection` - Verificar conexión
- `GET /api/news/get-latest` - Obtener últimas noticias
- `GET /api/news/get-top-headlines` - Titulares principales
- `POST /api/news/search` - Búsqueda personalizada

### Alertas
- `GET /api/app/news-alert-list` - Listar alertas del usuario
- `POST /api/app/news-alert-list` - Crear nueva alerta
- `PUT /api/app/news-alert-list/{id}` - Actualizar alerta
- `DELETE /api/app/news-alert-list/{id}` - Eliminar alerta
- `POST /api/app/news-alert-list/test-alert/{id}` - Probar alerta

### Notificaciones
- `GET /api/app/news-alert/my-notifications` - Obtener notificaciones
- `PUT /api/app/news-alert/mark-as-read/{id}` - Marcar como leída
- `PUT /api/app/news-alert/mark-all-as-read` - Marcar todas como leídas

### Perfil de Usuario
- `GET /api/app/user-profile` - Obtener perfil
- `PUT /api/app/user-profile` - Actualizar perfil
- `POST /api/app/user-profile/upload-photo` - Subir foto

---

## 🧪 Testing

### Backend Tests

```bash
cd test/NewsApp.Application.Tests
dotnet test
```

### Frontend Tests

```bash
cd NewsApp.Angular
npm test
```

---

## 🐛 Troubleshooting

### Error: "Cannot connect to SQL Server"
- Verifica que SQL Server esté ejecutándose
- Comprueba la cadena de conexión en `appsettings.json`
- Asegúrate de que el usuario tenga permisos en la base de datos

### Error: "NewsAPI request failed"
- Verifica que tu API key sea válida
- Comprueba que no hayas excedido el límite de requests (100/día en plan gratuito)
- Revisa la configuración en `appsettings.json`

### Error: "CORS policy blocked"
- Verifica que el backend esté corriendo en `https://localhost:44341`
- Comprueba que el frontend esté en `http://localhost:4200`
- Revisa la configuración CORS en `Program.cs` del backend

### Error: "Email not confirmed"
- Para testing, usa el endpoint `/api/app/notification/send-test-notification`
- En producción, confirma el email desde el enlace enviado al registrarse

---

## 📝 Notas de Desarrollo

### Convenciones de Código
- Backend: PascalCase para métodos y propiedades públicas
- Frontend: camelCase para variables y métodos
- Commits: Usar mensajes descriptivos en español

### Branches
- `main`: Código en producción
- `develop`: Desarrollo activo
- `feature/*`: Nuevas funcionalidades

---

## 📚 Documentación Adicional

- [ABP Framework Documentation](https://docs.abp.io/)
- [Angular Documentation](https://angular.io/docs)
- [NewsAPI Documentation](https://newsapi.org/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

---

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo licencia MIT para fines educativos.

---

## 👥 Contacto

Para preguntas o sugerencias sobre el proyecto, contactar a:
- **Email**: pablo.leal224@gmail.com
- **Universidad**: UTN-FRCU

---

## 🙏 Agradecimientos

- A los profesores Enzo Tanga y Esteban Tripodi por su guía
- A la UTN-FRCU por brindar las herramientas necesarias
- A la comunidad de ABP Framework por su excelente documentación
