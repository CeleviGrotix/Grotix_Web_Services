ALTER TABLE sensor RENAME COLUMN "Id" TO id;
ALTER TABLE sensor RENAME COLUMN "DeviceId" TO device_id;
ALTER TABLE sensor RENAME COLUMN "ZoneId" TO zone_id;
ALTER TABLE sensor RENAME COLUMN "Type" TO type;
ALTER TABLE sensor RENAME COLUMN "Unit" TO unit;
ALTER TABLE sensor RENAME COLUMN "MinPhysical" TO min_physical;
ALTER TABLE sensor RENAME COLUMN "MaxPhysical" TO max_physical;

ALTER TABLE sensor_reading RENAME COLUMN "SensorId" TO sensor_id;
ALTER TABLE sensor_reading RENAME COLUMN "Timestamp" TO timestamp;
ALTER TABLE sensor_reading RENAME COLUMN "Value" TO value;

ALTER TABLE active_thresholds RENAME COLUMN "ZoneId" TO zone_id;
ALTER TABLE active_thresholds RENAME COLUMN "SensorType" TO sensor_type;
ALTER TABLE active_thresholds RENAME COLUMN "MinValue" TO min_value;
ALTER TABLE active_thresholds RENAME COLUMN "MaxValue" TO max_value;

ALTER TABLE threshold_breach_tracker RENAME COLUMN "ZoneId" TO zone_id;
ALTER TABLE threshold_breach_tracker RENAME COLUMN "SensorId" TO sensor_id;
ALTER TABLE threshold_breach_tracker RENAME COLUMN "SensorType" TO sensor_type;
ALTER TABLE threshold_breach_tracker RENAME COLUMN "ConsecutiveCount" TO consecutive_count;
ALTER TABLE threshold_breach_tracker RENAME COLUMN "LastEvaluatedAt" TO last_evaluated_at;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260527140000_TelemetrySnakeCaseColumns', '9.0.0')
ON CONFLICT DO NOTHING;

SELECT create_hypertable('sensor_reading', 'timestamp', if_not_exists => TRUE);
