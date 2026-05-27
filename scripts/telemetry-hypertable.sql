ALTER TABLE sensor_reading DROP CONSTRAINT IF EXISTS "PK_sensor_reading";
SELECT create_hypertable('sensor_reading', 'timestamp', if_not_exists => TRUE);
ALTER TABLE sensor_reading ADD CONSTRAINT "PK_sensor_reading" PRIMARY KEY (sensor_id, timestamp);
