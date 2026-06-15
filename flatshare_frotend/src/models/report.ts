export type ReportType = "LISTING" | "USER";

export type ReportStatus =
  | "Open"
  | "UnderReview"
  | "ActionTaken"
  | "ClosedNoAction";

export type ViolationReportDTO = {
  id: string;
  type: ReportType;
  targetId: string;
  reason: string;
  details: string;
  status: ReportStatus;
  createdAt: string;
};

export type CreateReportBody = {
  type: ReportType;
  targetId: string;
  reason: string;
  details: string;
};

export type PageMetadata = {
  size: number;
  number: number;
  totalElements: number;
  totalPages: number;
};

export type PageResponse<T> = {
  content: T[];
  page: PageMetadata;
};
