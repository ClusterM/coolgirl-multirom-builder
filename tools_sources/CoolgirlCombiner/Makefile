# Makefile to create some resources

COMMIT=$(shell git rev-parse --short HEAD)
COMMIT_RESOURCE=Resources/commit.txt
BUILDTIME_RESOURCE=Resources/buildtime.txt

all: commit buildtime

commit:
	echo -n $(COMMIT) > $(COMMIT_RESOURCE)
	git diff-index --quiet HEAD -- || echo -n " (dirty)" >> $(COMMIT_RESOURCE)

buildtime:
	date -u +"%s" > $(BUILDTIME_RESOURCE)
