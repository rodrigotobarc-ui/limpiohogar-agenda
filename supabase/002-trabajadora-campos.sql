-- =============================================================
-- LimpioHogar - Modificar campos de Trabajadora
-- Ejecutar en Supabase SQL Editor
-- =============================================================

-- Eliminar campos que ya no se usan
ALTER TABLE trabajadoras DROP COLUMN IF EXISTS email;
ALTER TABLE trabajadoras DROP COLUMN IF EXISTS direccion;
ALTER TABLE trabajadoras DROP COLUMN IF EXISTS comuna;

-- Agregar metro cercano
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS metro_cercano text;

-- Agregar datos bancarios
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco_nombre_titular text;
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco_rut text;
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco text;
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco_tipo_cuenta text;
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco_numero_cuenta text;
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS banco_correo text;
