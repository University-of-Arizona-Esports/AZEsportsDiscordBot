CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS "user" (
  id INT PRIMARY KEY,
  username TEXT NOT NULL,
  discriminator TEXT NOT NULL,
  joined_at TIMESTAMP NOT NULL,
  avatar TEXT,
  bot BOOLEAN NOT NULL,
  system BOOLEAN NOT NULL,
  mfa_enabled BOOLEAN NOT NULL,
  locale TEXT NOT NULL,
  verified BOOLEAN NOT NULL,
  email TEXT,
  flags INT,
  premium_type INT,
  public_flags INT
);

CREATE TABLE IF NOT EXISTS channel (
  id INT PRIMARY KEY,
  type INT NOT NULL,
  guild_id INT,
  position INT,
  name TEXT NOT NULL,
  topic TEXT,
  nsfw BOOLEAN NOT NULL,
  bitrate INT NOT NULL,
  user_limit INT,
  rate_limit_per_user INT NOT NULL,
  icon TEXT,
  owner_id INT,
  application_id INT,
  parent_id INT
);

CREATE TABLE IF NOT EXISTS message (
  id INT PRIMARY KEY,
  channel_id INT NOT NULL,
  guild_id INT,
  author_id INT NOT NULL,
  content TEXT NOT NULL,
  "timestamp" TIMESTAMP NOT NULL,
  tts BOOLEAN NOT NULL,
  mention_everyone BOOLEAN NOT NULL,
  type INT NOT NULL
);

CREATE TABLE IF NOT EXISTS role (
  id INT PRIMARY KEY,
  name TEXT NOT NULL,
  color INT,
  position INT NOT NULL,
  permissions TEXT NOT NULL,
  managed BOOLEAN NOT NULL,
  mentionable BOOLEAN NOT NULL
);

CREATE TABLE IF NOT EXISTS config (
  join_message TEXT NOT NULL,
  welcome_channel_id INT NOT NULL,
  prefix TEXT NOT NULL,
  embed_color TEXT NOT NULL,
  playing_status TEXT,
  footer TEXT,
  log_channel_id INT NOT NULL
);

CREATE TABLE IF NOT EXISTS intrinsic_command (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  trigger TEXT NOT NULL,
  allow_pm BOOLEAN DEFAULT false,
  hidden BOOLEAN DEFAULT false,
  help TEXT
);

CREATE TABLE IF NOT EXISTS command (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  trigger TEXT NOT NULL,
  allow_pm BOOLEAN DEFAULT false,
  hidden BOOLEAN DEFAULT false,
  help TEXT,
  response TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS command_permission (
  command_id UUID,
  role_id INT,
  PRIMARY KEY (command_id, role_id)
);

CREATE TABLE IF NOT EXISTS command_channel (
  command_id UUID,
  channel_id INT,
  PRIMARY KEY (command_id, channel_id)
);

CREATE TABLE IF NOT EXISTS timed_message (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  channel_id INT NOT NULL,
  interval_mins INT NOT NULL DEFAULT 5,
  "text" TEXT NOT NULL DEFAULT 'None',
  start TIMESTAMP NOT NULL,
  "end" TIMESTAMP NOT NULL,
  last_use TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS reminder (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id INT NOT NULL,
  remind_at TIMESTAMP NOT NULL,
  reminder_text TEXT NOT NULL DEFAULT 'None',
  channel INT NOT NULL
);

CREATE TABLE IF NOT EXISTS mod_action (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  mod_id INT NOT NULL,
  user_id INT NOT NULL,
  action_type INT NOT NULL,
  reason TEXT
  -- Still working on this one,
  -- not sure what other fields are necessary

  -- maybe a `context` field storing relevant messages
  -- around the time frame of the action?
);

CREATE TABLE IF NOT EXISTS member_note (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id INT NOT NULL,
  note TEXT NOT NULL DEFAULT 'None'
);

CREATE TABLE IF NOT EXISTS faq (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  tag TEXT NOT NULL DEFAULT 'None',
  response TEXT NOT NULL DEFAULT 'None'
);

CREATE TABLE IF NOT EXISTS role_reaction_message (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  message TEXT NOT NULL DEFAULT 'None',
  image BYTEA,
  embed BOOLEAN NOT NULL DEFAULT true,
  message_id INT,
  image_message_id INT,
  channel_id INT NOT NULL,
  started BOOLEAN NOT NULL DEFAULT false,
  image_name TEXT
);

CREATE TABLE IF NOT EXISTS role_reaction (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  emoji TEXT NOT NULL DEFAULT 'None',
  role_id INT NOT NULL,
  role_reaction_id UUID
);

CREATE TABLE IF NOT EXISTS raffle (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  channel_id INT,
  "text" TEXT NOT NULL DEFAULT 'None',
  prize TEXT NOT NULL DEFAULT 'None',
  join_type INT NOT NULL DEFAULT 0,
  join_string TEXT DEFAULT 'None',
  join_emoji TEXT DEFAULT 'None',
  start_at TIMESTAMP,
  end_at TIMESTAMP,
  winner_id INT,
  message_id INT,
  last_reminder_sent_at TIMESTAMP,
  send_reminders BOOLEAN NOT NULL DEFAULT false,
  reminder_interval_minutes INT NOT NULL DEFAULT 5
);

CREATE TABLE IF NOT EXISTS raffle_entry (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  entered_on TIMESTAMP NOT NULL,
  raffle_id UUID NOT NULL,
  user_id INT NOT NULL
);

CREATE TABLE IF NOT EXISTS poll (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  is_open BOOLEAN NOT NULL DEFAULT false,
  description TEXT NOT NULL DEFAULT 'None',
  message_id INT,
  channel_id INT
);

CREATE TABLE IF NOT EXISTS poll_option (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  poll_id UUID NOT NULL,
  token TEXT NOT NULL DEFAULT 'None',
  description TEXT NOT NULL DEFAULT 'None'
);

CREATE TABLE IF NOT EXISTS poll_response (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  poll_id UUID NOT NULL,
  poll_option_id UUID NOT NULL,
  user_id INT NOT NULL
);

CREATE TABLE IF NOT EXISTS discord_oauth (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  user_id INT NOT NULL,
  access_token TEXT NOT NULL,
  token_type TEXT NOT NULL,
  expires_on TIMESTAMP NOT NULL,
  refresh_token TEXT NOT NULL,
  scope TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS user_xp (
  -- TODO: Create User XP table schema if necessary
);

CREATE TABLE IF NOT EXISTS locked_channel (
  channel_id INT PRIMARY KEY,
  unlock_at TIMESTAMP
);

ALTER TABLE channel ADD CONSTRAINT owner_id FOREIGN KEY (owner_id) REFERENCES "user" (id);
ALTER TABLE message ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE message ADD CONSTRAINT author_id FOREIGN KEY (author_id) REFERENCES "user" (id);
ALTER TABLE config ADD CONSTRAINT welcome_channel_id FOREIGN KEY (welcome_channel_id) REFERENCES channel (id);
ALTER TABLE config ADD CONSTRAINT log_channel_id FOREIGN KEY (log_channel_id) REFERENCES channel (id);
ALTER TABLE command_permission ADD CONSTRAINT command_id FOREIGN KEY (command_id) REFERENCES command (id);
ALTER TABLE command_permission ADD CONSTRAINT role_id FOREIGN KEY (role_id) REFERENCES role (id);
ALTER TABLE command_channel ADD CONSTRAINT command_id FOREIGN KEY (command_id) REFERENCES command (id);
ALTER TABLE command_channel ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE timed_message ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE reminder ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES "user" (id);
ALTER TABLE reminder ADD CONSTRAINT channel FOREIGN KEY (channel) REFERENCES channel (id);
ALTER TABLE mod_action ADD CONSTRAINT mod_id FOREIGN KEY (mod_id) REFERENCES "user" (id);
ALTER TABLE mod_action ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES "user" (id);
ALTER TABLE member_note ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES "user" (id);
ALTER TABLE role_reaction_message ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE role_reaction ADD CONSTRAINT role_id FOREIGN KEY (role_id) REFERENCES role (id);
ALTER TABLE role_reaction ADD CONSTRAINT role_reaction_id FOREIGN KEY (role_reaction_id) REFERENCES role_reaction_message (id);
ALTER TABLE raffle ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE raffle ADD CONSTRAINT winner_id FOREIGN KEY (winner_id) REFERENCES "user" (id);
ALTER TABLE raffle_entry ADD CONSTRAINT raffle_id FOREIGN KEY (raffle_id) REFERENCES raffle (id);
ALTER TABLE RAFFLE_ENTRY ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES "user" (id);
ALTER TABLE poll ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
ALTER TABLE poll_option ADD CONSTRAINT poll_id FOREIGN KEY (poll_id) REFERENCES poll (id);
ALTER TABLE poll_response ADD CONSTRAINT poll_id FOREIGN KEY (poll_id) REFERENCES poll (id);
ALTER TABLE poll_response ADD CONSTRAINT poll_option_id FOREIGN KEY (poll_option_id) REFERENCES poll_option (id);
ALTER TABLE poll_response ADD CONSTRAINT user_id FOREIGN KEY (user_id) REFERENCES "user" (id);
ALTER TABLE locked_channel ADD CONSTRAINT channel_id FOREIGN KEY (channel_id) REFERENCES channel (id);
