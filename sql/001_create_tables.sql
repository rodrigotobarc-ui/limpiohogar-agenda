-- ============================================
-- LimpioHogar Agenda - Esquema de base de datos
-- Ejecutar en Supabase SQL Editor
-- ============================================

-- Extensión UUID
create extension if not exists "uuid-ossp";

-- ============================================
-- Tabla: trabajadoras
-- ============================================
create table if not exists trabajadoras (
  id uuid primary key default uuid_generate_v4(),
  nombre text not null,
  apellido text not null,
  telefono text not null,
  email text,
  direccion text,
  comuna text,
  estado_migratorio text not null default 'no_informado',
  activa boolean not null default true,
  notas text,
  created_at timestamptz not null default now()
);

-- ============================================
-- Tabla: clientes
-- ============================================
create table if not exists clientes (
  id uuid primary key default uuid_generate_v4(),
  nombre text not null,
  apellido text not null,
  telefono text not null,
  email text,
  direccion text not null,
  comuna text not null,
  tipo text not null default 'casa',
  notas text,
  activo boolean not null default true,
  created_at timestamptz not null default now()
);

-- ============================================
-- Tabla: servicios
-- ============================================
create table if not exists servicios (
  id uuid primary key default uuid_generate_v4(),
  trabajadora_id uuid not null references trabajadoras(id) on delete restrict,
  cliente_id uuid not null references clientes(id) on delete restrict,
  fecha date not null,
  hora time not null,
  plan text not null default 'basico',
  estado text not null default 'pendiente',
  monto_total integer not null,
  monto_trabajadora integer not null,
  monto_agencia integer not null,
  notas text,
  created_at timestamptz not null default now()
);

-- ============================================
-- Tabla: pagos
-- ============================================
create table if not exists pagos (
  id uuid primary key default uuid_generate_v4(),
  servicio_id uuid not null references servicios(id) on delete cascade,
  fecha_pago date not null,
  monto integer not null,
  tipo text not null,
  confirmado boolean not null default false,
  notas text,
  created_at timestamptz not null default now()
);

-- ============================================
-- Índices
-- ============================================
create index if not exists idx_servicios_fecha on servicios(fecha);
create index if not exists idx_servicios_trabajadora on servicios(trabajadora_id);
create index if not exists idx_servicios_cliente on servicios(cliente_id);
create index if not exists idx_pagos_servicio on pagos(servicio_id);
create index if not exists idx_pagos_fecha on pagos(fecha_pago);

-- ============================================
-- Row Level Security (RLS)
-- Habilitar RLS y crear política pública (anon key)
-- ============================================
alter table trabajadoras enable row level security;
alter table clientes enable row level security;
alter table servicios enable row level security;
alter table pagos enable row level security;

-- Políticas de acceso público (para usar con anon key sin auth)
create policy "Acceso público trabajadoras" on trabajadoras for all using (true) with check (true);
create policy "Acceso público clientes" on clientes for all using (true) with check (true);
create policy "Acceso público servicios" on servicios for all using (true) with check (true);
create policy "Acceso público pagos" on pagos for all using (true) with check (true);
