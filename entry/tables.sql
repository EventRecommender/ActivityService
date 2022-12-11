CREATE TABLE activity (
    id INT AUTO_INCREMENT NOT NULL,
    title TEXT NOT NULL,
    host VARCHAR(255) NOT NULL,
    place VARCHAR(255) NOT NULL,
    time TIMESTAMP NOT NULL,
    img VARCHAR(255) NOT NULL,
    path VARCHAR(255) NOT NULL,
    description VARCHAR(255) NOT NULL,
    active BOOLEAN NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE type (
    activityid INT NOT NULL,
    tag VARCHAR(255) NOT NULL,
    FOREIGN KEY (activityid) REFERENCES activity(id)
    ON DELETE CASCADE
);

CREATE TABLE tags (
    tag VARCHAR(255) NOT NULL,
    PRIMARY KEY (tag)
);