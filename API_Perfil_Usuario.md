# API de Perfil de Usuario - NewsApp

## Descripción
La funcionalidad de perfil de usuario permite a los usuarios autenticados gestionar su información personal, cambiar contraseñas y configurar sus preferencias de idioma para las noticias.

## Endpoints Disponibles

### 1. Obtener Perfil del Usuario Actual
**GET** `/api/app/user-profile`

**Descripción:** Obtiene la información del perfil del usuario autenticado.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Respuesta exitosa (200):**
```json
{
  "id": "guid",
  "email": "usuario@ejemplo.com",
  "userName": "nombreusuario",
  "name": "Nombre",
  "surname": "Apellido",
  "phoneNumber": "+1234567890",
  "emailConfirmed": true,
  "phoneNumberConfirmed": false,
  "newsLanguageCode": "es",
  "newsLanguageName": "Español",
  "twoFactorEnabled": false,
  "creationTime": "2025-01-11T20:30:00Z",
  "lastModificationTime": "2025-01-11T21:00:00Z"
}
```

### 2. Actualizar Perfil del Usuario
**PUT** `/api/app/user-profile`

**Descripción:** Actualiza la información del perfil del usuario autenticado.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "email": "nuevoemail@ejemplo.com",
  "userName": "nuevonombreusuario",
  "name": "Nuevo Nombre",
  "surname": "Nuevo Apellido",
  "phoneNumber": "+0987654321",
  "newsLanguageCode": "en"
}
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Perfil actualizado correctamente",
  "requiresEmailConfirmation": true,
  "profile": {
    // ... datos del perfil actualizado
  }
}
```

### 3. Cambiar Contraseña
**POST** `/api/app/user-profile/change-password`

**Descripción:** Cambia la contraseña del usuario autenticado.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "currentPassword": "contraseñaactual123",
  "newPassword": "nuevacontraseña456",
  "confirmNewPassword": "nuevacontraseña456"
}
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Contraseña cambiada correctamente",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

### 4. Obtener Idiomas Disponibles
**GET** `/api/app/user-profile/news-languages`

**Descripción:** Obtiene la lista de idiomas disponibles para las noticias.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Respuesta exitosa (200):**
```json
[
  {
    "code": "en",
    "name": "English",
    "isSupported": true
  },
  {
    "code": "es",
    "name": "Español",
    "isSupported": true
  },
  {
    "code": "fr",
    "name": "Français",
    "isSupported": true
  }
  // ... más idiomas
]
```

### 5. Actualizar Idioma de Noticias
**POST** `/api/app/user-profile/news-language`

**Descripción:** Actualiza el idioma preferido del usuario para las noticias.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
"es"
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Idioma de noticias actualizado correctamente",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

### 6. Enviar Confirmación de Email
**POST** `/api/app/user-profile/send-email-confirmation`

**Descripción:** Envía un email de confirmación al usuario.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{}
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Confirmación de correo enviada correctamente",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

### 7. Confirmar Email
**POST** `/api/app/user-profile/confirm-email`

**Descripción:** Confirma el email del usuario usando un token.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
"token-de-confirmacion-aqui"
```

**Respuesta exitosa (200):**
```json
{
  "success": true,
  "message": "Correo electrónico confirmado correctamente",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

## Códigos de Error Comunes

### 400 Bad Request
```json
{
  "success": false,
  "message": "El nombre de usuario ya existe",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

### 401 Unauthorized
```json
{
  "error": {
    "code": "401",
    "message": "Token de autorización requerido"
  }
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Error interno del servidor",
  "requiresEmailConfirmation": false,
  "profile": null
}
```

## Notas Importantes

1. **Autenticación:** Todos los endpoints requieren autenticación mediante Bearer Token.

2. **Validaciones:**
   - El email debe tener un formato válido
   - El nombre de usuario es requerido y debe ser único
   - Las contraseñas deben tener al menos 6 caracteres
   - La confirmación de contraseña debe coincidir con la nueva contraseña

3. **Confirmación de Email:** 
   - Si cambias el email, se requiere confirmación
   - El email se marca como no confirmado hasta que uses el token

4. **Idiomas Soportados:** 
   - Los idiomas disponibles incluyen español, inglés, francés, alemán, italiano, portugués, ruso, chino, japonés, coreano, hindi, turco, holandés, sueco, noruego y danés

5. **Persistencia:** 
   - Los cambios en el perfil se guardan inmediatamente
   - Las preferencias de idioma se aplican a futuras búsquedas de noticias

## Probar la API

Para probar la API, puedes usar:

1. **Swagger UI:** Visita `https://localhost:44367/swagger` (después de ejecutar la aplicación)
2. **Postman:** Importa las configuraciones de los endpoints
3. **curl:** Usar comandos curl desde la terminal

### Ejemplo con curl:
```bash
# Obtener perfil
curl -X GET "https://localhost:44367/api/app/user-profile" \
  -H "Authorization: Bearer tu-token-aqui" \
  -H "Content-Type: application/json"

# Actualizar perfil
curl -X PUT "https://localhost:44367/api/app/user-profile" \
  -H "Authorization: Bearer tu-token-aqui" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "nuevo@ejemplo.com",
    "userName": "nuevousuario",
    "name": "Nombre",
    "surname": "Apellido",
    "newsLanguageCode": "es"
  }'
```