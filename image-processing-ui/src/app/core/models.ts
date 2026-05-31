/** Processing status of an image. Numeric values match the server enum. */
export enum ImageStatus {
  InProcess = 0,
  Finished = 1,
  ProcessError = 2,
}

/** Summary of an image, as returned by GET /images. */
export interface ImageListItem {
  id: string;
  fileName: string;
  width: number;
  height: number;
  status: ImageStatus;
  downloadUrl: string;
}

/** Full details of an image, as returned by GET /images/{id}. */
export interface ImageDetails {
  id: string;
  fileName: string;
  extension: string;
  width: number;
  height: number;
  fileSize: number;
  status: ImageStatus;
  pipelineHistory: string[];
}

/** Active-image count for a single pipeline, as returned by GET /pipelines. */
export interface PipelineStatus {
  pipeline: string;
  activeImages: number;
}
