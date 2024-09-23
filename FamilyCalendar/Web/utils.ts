type IsoDateString = string & { __brand: "ISO8601DateString" };

const dateTimeLocalFormat = "yyyy-MM-dd'T'HH:mm";

function getLocalIsoDate(): IsoDateString {
  return luxon.DateTime.local().toISO() as IsoDateString;
}

function getLocalTimeZone(): string {
  return luxon.DateTime.local().zoneName;
}

function formatIsoDateForDisplay(date: IsoDateString): string {
  return luxon.DateTime.fromISO(date).toFormat("f");
}

function formatIsoDateForDateTimeLocalInput(date: IsoDateString): string {
  return luxon.DateTime.fromISO(date).toLocal().toFormat(dateTimeLocalFormat);
}

function formatDateTimeLocalToUtcString(date: string): IsoDateString {
  return luxon.DateTime.fromFormat(date, dateTimeLocalFormat)
    .toUTC()
    .toISO() as IsoDateString;
}
