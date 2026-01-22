-- Script para actualizar el email del usuario admin
-- Ejecuta esto en SQL Server Management Studio o Azure Data Studio

USE NewsApp;
GO

-- Ver el email actual del admin
SELECT Id, UserName, Email, EmailConfirmed 
FROM AbpUsers 
WHERE UserName = 'admin';

-- Actualizar el email del admin a tu email real
UPDATE AbpUsers 
SET Email = 'nachomaartinez@gmail.com',
    NormalizedEmail = 'NACHOMAARTINEZ@GMAIL.COM',
    EmailConfirmed = 0  -- Marca como no confirmado para que puedas confirmar después
WHERE UserName = 'admin';

-- Verificar que se actualizó
SELECT Id, UserName, Email, EmailConfirmed 
FROM AbpUsers 
WHERE UserName = 'admin';

GO
