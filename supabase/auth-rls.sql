-- =============================================================
-- LimpioHogar Agenda PWA - Auth & RLS Migration
-- Ejecutar en Supabase SQL Editor (una sola vez)
-- =============================================================

-- 1. Agregar columna user_id a trabajadoras y clientes
ALTER TABLE trabajadoras ADD COLUMN IF NOT EXISTS user_id uuid REFERENCES auth.users(id);
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS user_id uuid REFERENCES auth.users(id);

-- 2. Backfill: asignar datos existentes al admin
UPDATE trabajadoras SET user_id = '0817ba54-3932-4d51-b146-3a2d6a8614d2' WHERE user_id IS NULL;
UPDATE clientes SET user_id = '0817ba54-3932-4d51-b146-3a2d6a8614d2' WHERE user_id IS NULL;

-- 3. Hacer NOT NULL despues del backfill
ALTER TABLE trabajadoras ALTER COLUMN user_id SET NOT NULL;
ALTER TABLE clientes ALTER COLUMN user_id SET NOT NULL;

-- 4. Indice para performance en RLS
CREATE INDEX IF NOT EXISTS idx_trabajadoras_user_id ON trabajadoras(user_id);
CREATE INDEX IF NOT EXISTS idx_clientes_user_id ON clientes(user_id);

-- 5. Marcar admin en app_metadata del usuario
UPDATE auth.users
SET raw_app_meta_data = COALESCE(raw_app_meta_data, '{}'::jsonb) || '{"role": "admin"}'::jsonb
WHERE id = '0817ba54-3932-4d51-b146-3a2d6a8614d2';

-- 6. Funcion helper is_admin()
CREATE OR REPLACE FUNCTION public.is_admin()
RETURNS boolean
LANGUAGE sql STABLE SECURITY DEFINER
AS $$
  SELECT COALESCE(
    (auth.jwt() -> 'app_metadata' ->> 'role') = 'admin',
    false
  );
$$;

-- =============================================================
-- 7. Habilitar RLS en todas las tablas
-- =============================================================
ALTER TABLE trabajadoras ENABLE ROW LEVEL SECURITY;
ALTER TABLE clientes ENABLE ROW LEVEL SECURITY;
ALTER TABLE servicios ENABLE ROW LEVEL SECURITY;
ALTER TABLE pagos ENABLE ROW LEVEL SECURITY;

-- =============================================================
-- 8. Policies para TRABAJADORAS (filtro directo por user_id)
-- =============================================================
CREATE POLICY "trabajadoras_select" ON trabajadoras FOR SELECT
  USING (user_id = auth.uid() OR is_admin());

CREATE POLICY "trabajadoras_insert" ON trabajadoras FOR INSERT
  WITH CHECK (user_id = auth.uid() OR is_admin());

CREATE POLICY "trabajadoras_update" ON trabajadoras FOR UPDATE
  USING (user_id = auth.uid() OR is_admin());

CREATE POLICY "trabajadoras_delete" ON trabajadoras FOR DELETE
  USING (user_id = auth.uid() OR is_admin());

-- =============================================================
-- 9. Policies para CLIENTES (filtro directo por user_id)
-- =============================================================
CREATE POLICY "clientes_select" ON clientes FOR SELECT
  USING (user_id = auth.uid() OR is_admin());

CREATE POLICY "clientes_insert" ON clientes FOR INSERT
  WITH CHECK (user_id = auth.uid() OR is_admin());

CREATE POLICY "clientes_update" ON clientes FOR UPDATE
  USING (user_id = auth.uid() OR is_admin());

CREATE POLICY "clientes_delete" ON clientes FOR DELETE
  USING (user_id = auth.uid() OR is_admin());

-- =============================================================
-- 10. Policies para SERVICIOS (cascada via trabajadora_id)
-- =============================================================
CREATE POLICY "servicios_select" ON servicios FOR SELECT
  USING (
    EXISTS (
      SELECT 1 FROM trabajadoras t
      WHERE t.id = servicios.trabajadora_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "servicios_insert" ON servicios FOR INSERT
  WITH CHECK (
    EXISTS (
      SELECT 1 FROM trabajadoras t
      WHERE t.id = servicios.trabajadora_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "servicios_update" ON servicios FOR UPDATE
  USING (
    EXISTS (
      SELECT 1 FROM trabajadoras t
      WHERE t.id = servicios.trabajadora_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "servicios_delete" ON servicios FOR DELETE
  USING (
    EXISTS (
      SELECT 1 FROM trabajadoras t
      WHERE t.id = servicios.trabajadora_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

-- =============================================================
-- 11. Policies para PAGOS (cascada via servicio -> trabajadora)
-- =============================================================
CREATE POLICY "pagos_select" ON pagos FOR SELECT
  USING (
    EXISTS (
      SELECT 1 FROM servicios s
      JOIN trabajadoras t ON t.id = s.trabajadora_id
      WHERE s.id = pagos.servicio_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "pagos_insert" ON pagos FOR INSERT
  WITH CHECK (
    EXISTS (
      SELECT 1 FROM servicios s
      JOIN trabajadoras t ON t.id = s.trabajadora_id
      WHERE s.id = pagos.servicio_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "pagos_update" ON pagos FOR UPDATE
  USING (
    EXISTS (
      SELECT 1 FROM servicios s
      JOIN trabajadoras t ON t.id = s.trabajadora_id
      WHERE s.id = pagos.servicio_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );

CREATE POLICY "pagos_delete" ON pagos FOR DELETE
  USING (
    EXISTS (
      SELECT 1 FROM servicios s
      JOIN trabajadoras t ON t.id = s.trabajadora_id
      WHERE s.id = pagos.servicio_id
        AND (t.user_id = auth.uid() OR is_admin())
    )
  );
