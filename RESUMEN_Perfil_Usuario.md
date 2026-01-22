# Funcionalidad de Perfil de Usuario - NewsApp

## ?? Resumen de Implementación

He implementado una funcionalidad completa de perfil de usuario que permite:

### ? Backend (C# - ABP Framework)

**1. Modelos y DTOs:**
- `UserProfileDto` - Información del perfil del usuario
- `UpdateUserProfileDto` - Datos para actualizar el perfil
- `ChangePasswordDto` - Datos para cambio de contraseña
- `NewsLanguageDto` - Idiomas disponibles para noticias
- `ProfileUpdateResultDto` - Resultado de operaciones

**2. Entidad de Dominio:**
- `UserPreferences` - Almacena preferencias del usuario (idioma de noticias, notificaciones, etc.)
- `IUserPreferencesRepository` - Repositorio para gestionar preferencias

**3. Servicios de Aplicación:**
- `IUserProfileAppService` - Interfaz del servicio
- `UserProfileAppService` - Implementación del servicio con:
  - Obtener perfil del usuario
  - Actualizar información personal
  - Cambiar contraseña
  - Gestionar idioma de noticias
  - Confirmación de email

**4. API Controllers:**
- `UserProfileController` - Endpoints REST para todas las operaciones

**5. Base de Datos:**
- Nueva tabla `AppUserPreferences` creada mediante migración
- Configuración en `NewsAppDbContext`

**6. Localización:**
- Mensajes en inglés y español para todas las operaciones
- Archivos `en.json` y `es.json` actualizados

### ? Frontend (Angular)

**1. Modelos TypeScript:**
- Interfaces para todos los DTOs del backend
- Tipos para manejar respuestas de la API

**2. Servicio Angular:**
- `UserProfileService` - Cliente HTTP para comunicarse con la API
- Métodos para todas las operaciones CRUD

**3. Componentes:**
- `UserProfileComponent` - Página completa del perfil
- `UserProfileModalComponent` - Modal para acceso rápido desde el header

**4. UI/UX:**
- Interfaz con pestañas: Información Personal, Cambiar Contraseña, Preferencias
- Validaciones en tiempo real
- Mensajes de éxito/error
- Dropdown en el header del usuario con acceso al perfil

## ?? Características Implementadas

### Gestión de Información Personal
- ? Editar nombre de usuario (con validación de unicidad)
- ? Editar correo electrónico (con confirmación automática)
- ? Editar nombre y apellido
- ? Editar número de teléfono
- ? Estado de confirmación de email visible

### Gestión de Contraseña
- ? Cambio de contraseña con validación de contraseña actual
- ? Validación de nueva contraseña (mínimo 6 caracteres)
- ? Confirmación de nueva contraseña
- ? Encriptación segura usando ABP Identity

### Preferencias de Idioma
- ? Selección de idioma para noticias
- ? 17 idiomas soportados (español, inglés, francés, alemán, etc.)
- ? Persistencia de preferencias en base de datos

### Experiencia de Usuario
- ? Modal desde el header del usuario (junto al logout)
- ? Dropdown menu con opciones de perfil y logout
- ? Interfaz responsive para móviles
- ? Validaciones en tiempo real
- ? Mensajes de feedback claros

## ?? Cómo Usar la Funcionalidad

### Para el Usuario Final:

1. **Acceder al Perfil:**
   - Hacer clic en el nombre de usuario en el header
   - Seleccionar "Mi Perfil" del dropdown

2. **Editar Información:**
   - Pestaña "Información Personal"
   - Modificar los campos deseados
   - Hacer clic en "Guardar Cambios"

3. **Cambiar Contraseña:**
   - Pestaña "Cambiar Contraseña"
   - Ingresar contraseña actual y nueva contraseña
   - Hacer clic en "Cambiar Contraseña"

4. **Configurar Idioma:**
   - Pestaña "Preferencias"
   - Seleccionar idioma preferido para noticias
   - Hacer clic en "Guardar Preferencias"

### Para Desarrolladores:

1. **Ejecutar Migraciones:**
   ```bash
   cd src/NewsApp.DbMigrator
   dotnet run
   ```

2. **Iniciar Backend:**
   ```bash
   cd src/NewsApp.HttpApi.Host
   dotnet run
   ```

3. **Probar API:**
   - Swagger UI: `https://localhost:44367/swagger`
   - Endpoints disponibles en `/api/app/user-profile`

## ?? Endpoints de la API

- `GET /api/app/user-profile` - Obtener perfil
- `PUT /api/app/user-profile` - Actualizar perfil
- `POST /api/app/user-profile/change-password` - Cambiar contraseña
- `GET /api/app/user-profile/news-languages` - Idiomas disponibles
- `POST /api/app/user-profile/news-language` - Actualizar idioma
- `POST /api/app/user-profile/send-email-confirmation` - Enviar confirmación
- `POST /api/app/user-profile/confirm-email` - Confirmar email

## ?? Seguridad

- ? Autenticación requerida para todos los endpoints
- ? Autorización basada en el usuario actual
- ? Validación de contraseña actual antes del cambio
- ? Encriptación de contraseñas usando ABP Identity
- ? Validación de datos en frontend y backend

## ?? Responsive Design

La interfaz está optimizada para:
- ? Desktop (diseño de 2 columnas)
- ? Tablet (diseño adaptativo)
- ? Móvil (diseño de 1 columna, pestañas verticales)

## ?? Internacionalización

- ? Soporte para múltiples idiomas en la UI
- ? Mensajes de error/éxito localizados
- ? 17 idiomas disponibles para contenido de noticias

## ??? Base de Datos

Nueva tabla creada: `AppUserPreferences`
```sql
- Id (Guid, PK)
- UserId (Guid, FK to AbpUsers)
- NewsLanguageCode (nvarchar(10))
- NewsLanguageName (nvarchar(50))
- EmailNotifications (bit)
- PushNotifications (bit)
- ArticlesPerPage (int)
- TimeZone (nvarchar(100))
- Theme (nvarchar(20))
- ShowImages (bit)
- AutoRefresh (bit)
- AutoRefreshInterval (int)
- CreationTime (datetime2)
- LastModificationTime (datetime2)
```

¡La funcionalidad está lista para usar! El usuario ahora puede acceder a su perfil desde el dropdown del header y gestionar toda su información personal, contraseña y preferencias de idioma.